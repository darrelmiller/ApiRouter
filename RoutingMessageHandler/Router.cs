using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace RoutingMessageHandler
{
    public class Router : DelegatingHandler
    {
        private readonly string _segmentTemplate;
        private readonly Dictionary<string, Router> _childRouters = new Dictionary<string, Router>();
        private bool _hasController;
        private Type _ControllerType;
        private HttpRouteValueDictionary _ConfiguredRouteData ;
        private readonly Dictionary<string, IHttpRouteConstraint> _constraints = new Dictionary<string, IHttpRouteConstraint>();
        private readonly Regex _MatchPattern;
        public Router ParentRouter { get; set; }

        private DelegatingHandler _MessageHandler;

        public Router(string segmentTemplate) 
        {
            _segmentTemplate = segmentTemplate;
            _MatchPattern = CreateMatchPattern(segmentTemplate);
            
        }

        private static Regex CreateMatchPattern(string segmentTemplate)
        {
            var pattern = new StringBuilder();

            StringBuilder paramName = null;
            foreach (char tcharacter in segmentTemplate)
            {
                
                if (tcharacter == '{')
                {
                    paramName = new StringBuilder();
                }

                if (tcharacter != '}' && tcharacter != '{')
                {
                    if (paramName == null)
                    {
                        pattern.Append(tcharacter);
                    }
                    else
                    {
                        paramName.Append(tcharacter);
                    }
                }
                if (tcharacter == '}')
                {

                    pattern.Append("(?<" + paramName.ToString() + ">.*)");
                    paramName = null;
                }
            }
            return new Regex(pattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public string SegmentTemplate
        {
            get { return _segmentTemplate; }
        }

        public Router WithConstraint(string parameterName, IHttpRouteConstraint constraint)
        {
            _constraints.Add(parameterName, constraint);
            return this;
        }
        public Router WithConstraint(string parameterName, string regexConstraint)
        {
            _constraints.Add(parameterName,new RegexConstraint(regexConstraint));
            
            return this;
        }

        public Router WithHandler(DelegatingHandler delegatingHandler)
        {
            // router -> MessageHandler -> router ->Child Router
            _MessageHandler = delegatingHandler;
            _MessageHandler.InnerHandler = this;
            InnerHandler = _MessageHandler; // This creates a crazy loop. The router has to know when it is re-entered to prevent the cyle.
                                            // Current innerhandler is a HttpControllerDispatcher which we aren't using, so throw away :-)
            
            
            return this;
        }
        public Router Add(Router childRouter)
        {
            
            _childRouters.Add(childRouter.SegmentTemplate, childRouter);
            
            childRouter.ParentRouter = this;    
            return this;
        }
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            PathRouteData pathRouteData;

            // Message handlers cause this method to be re-entrant.  Determine if this is the first time in or not.
            var beforeMessageHandler = IsBeforeMessageHandler(request, _MessageHandler);

            if (beforeMessageHandler || _MessageHandler == null)
            {
                pathRouteData = RetreiveRouteData(request);

                // Parse Parameters from URL and add them to the PathRouteData
                var result = _MatchPattern.Match(pathRouteData.CurrentSegment);

                for (int i = 1; i < result.Groups.Count; i++)
                {
                    var name = _MatchPattern.GroupNameFromNumber(i);
                    var value = result.Groups[i].Value;
                    pathRouteData.SetParameter(name, value);
                }
            } else
            {
                pathRouteData = (PathRouteData)GetRouteData(request);
            }

            if (beforeMessageHandler)
            {
                base.SendAsync(request, cancellationToken);  // Call message handler, which will call back into here.
            }



            if (pathRouteData.EndOfPath())
            {
                if (_hasController)
                {
                    return Dispatch(request, cancellationToken);
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            } else
            {
                pathRouteData.MoveToNext();

                var nextRouter = GetMatchingRouter(request, pathRouteData.CurrentSegment);

                if (nextRouter != null)
                {
                    return nextRouter.SendAsync(request, cancellationToken);
                }

                throw new HttpResponseException(HttpStatusCode.NotFound);

                
            }

        }

        internal Router GetMatchingRouter(HttpRequestMessage message, string template)
        {
            foreach (var childRouter in _childRouters.Values)
            {
                if (childRouter.Matches(message,template))
                {
                    return childRouter;
                }
            }

            return null;
           
        }

        private PathRouteData RetreiveRouteData(HttpRequestMessage request)
        {
            PathRouteData pathRouteData;

            if (request.Properties.ContainsKey(HttpPropertyKeys.HttpRouteDataKey))
            {
                //MVC Host requires a single route to trigger MessageHandlers to kick in
                // replace that RouteData with our PathRouteData. 
                var currentRouteData = GetRouteData(request);
                pathRouteData = currentRouteData as PathRouteData;

                if (pathRouteData == null)
                {
                    pathRouteData = new PathRouteData(request.RequestUri);

                    // Do we need to copy over the properties???
                    foreach (var value in currentRouteData.Values)
                    {
                        pathRouteData.Values.Add(value.Key, value.Value);
                    }

                    request.Properties[HttpPropertyKeys.HttpRouteDataKey] = pathRouteData;

                } 
                
            } else
            {
                // Self host will not create RouteData itself
                pathRouteData = new PathRouteData(request.RequestUri);
                request.Properties.Add(new KeyValuePair<string, object>(HttpPropertyKeys.HttpRouteDataKey, pathRouteData));
            }


            // Copy over values provided during Router setup
            if (_ConfiguredRouteData != null)
            {
                foreach (var value in _ConfiguredRouteData)
                {
                    pathRouteData.Values.Add(value.Key, value.Value);
                }
            }
            return pathRouteData;
        }

        private IHttpRouteData GetRouteData(HttpRequestMessage request)
        {
            return (IHttpRouteData)request.Properties[HttpPropertyKeys.HttpRouteDataKey];
        }

        internal bool Matches(HttpRequestMessage request, string paramTemplate)
        {
            var result = _MatchPattern.Match(paramTemplate);
            if (!result.Success)
                return false;


            var parameterValues = new Dictionary<string, object>();
            for (int i = 1; i < result.Groups.Count; i++)
            {
                var name = _MatchPattern.GroupNameFromNumber(i);
                var value = result.Groups[i].Value;
                parameterValues.Add(name, value);
            }

            foreach (var constraint in _constraints)
            {
                var match = constraint.Value.Match(request, null, constraint.Key, parameterValues, HttpRouteDirection.UriResolution);
                if (match == false)
                {
                    return false;
                }
            }

          
            
            
            return true;
            
        }
     

        private Task<HttpResponseMessage> Dispatch(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpConfiguration configuration = GetConfiguration(request);

            var controllerDescriptor = new HttpControllerDescriptor(configuration, _ControllerType.Name, _ControllerType);

            var controllerContext = new HttpControllerContext(configuration, GetRouteData(request), request);
            controllerContext.ControllerDescriptor = controllerDescriptor;

            IHttpControllerActivator activator = GetActivator(configuration);
            var httpController = activator.Create(controllerContext, _ControllerType);
            controllerContext.Controller = httpController;

            return httpController.ExecuteAsync(controllerContext, cancellationToken);

        }

        private IHttpControllerActivator GetActivator(HttpConfiguration configuration)
        {
            return (IHttpControllerActivator)configuration.ServiceResolver.GetService(typeof (IHttpControllerActivator));
        }

        private HttpConfiguration GetConfiguration(HttpRequestMessage request)
        {
            HttpConfiguration configuration = null;
            if (request.Properties.ContainsKey(HttpPropertyKeys.HttpConfigurationKey))
            {
                configuration = (HttpConfiguration)request.Properties[HttpPropertyKeys.HttpConfigurationKey];
            } else
            {
                configuration = new HttpConfiguration(); // empty config
            }
            return configuration;
        }


        public Router DispatchTo<T>()
        {
            _ControllerType = typeof(T);
            _hasController = true;
            return this;
        }
        public Router DispatchTo<T>(object routeValues)
        {
            _ConfiguredRouteData = new HttpRouteValueDictionary(routeValues);

            DispatchTo<T>();
            return this;
        }


        private bool IsBeforeMessageHandler(HttpRequestMessage request, DelegatingHandler handler)
        {
            if (_MessageHandler == null) return false;

            List<DelegatingHandler> delegatingHandlers;
            if (!request.Properties.ContainsKey("ProcessedMessageHandlers"))
            {
                delegatingHandlers = new List<DelegatingHandler>();
                request.Properties["ProcessedMessageHandlers"] = delegatingHandlers;
                delegatingHandlers.Add(handler);
                return true;
            } else
            {
                delegatingHandlers = (List<DelegatingHandler>)request.Properties["ProcessedMessageHandlers"];
            }

            if (delegatingHandlers.Contains(handler))
            {
                return false;
                
            }
            delegatingHandlers.Add(handler);
            return true;

        }
    }

    
   
    

    public class RegexConstraint : IHttpRouteConstraint
    {
        private readonly string _rule;

        public RegexConstraint(string rule)
        {
            _rule = rule;
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            object parameterValue;
            values.TryGetValue(parameterName, out parameterValue);
            string parameterValueString = Convert.ToString(parameterValue, CultureInfo.InvariantCulture);
            string constraintsRegEx = "^(" + _rule + ")$";
            return Regex.IsMatch(parameterValueString, constraintsRegEx, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        }
    }
} 