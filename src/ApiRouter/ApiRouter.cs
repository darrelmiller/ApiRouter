using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Linq;

namespace Tavis
{
    public class ApiRouter : DelegatingHandler, IHttpRoute
    {
        private readonly Uri _baseUrl;
        private readonly string _segmentTemplate;
        private readonly Dictionary<string, ApiRouter> _childRouters = new Dictionary<string, ApiRouter>();
        private bool _HasController;
        private Type _ControllerType;
        private HttpRouteValueDictionary _Defaults ;
        private readonly Dictionary<string, IHttpRouteConstraint> _Constraints = new Dictionary<string, IHttpRouteConstraint>();
        private readonly Regex _MatchPattern;
        public ApiRouter ParentRouter { get; set; }

        private DelegatingHandler _MessageHandler;
        private int _InitalPosition = 0;
        private string _ControllerInstanceName;
        private Dictionary<Type, Type> _Links = new Dictionary<Type, Type>();


        public ApiRouter(string segmentTemplate, Uri baseUrl) : this(segmentTemplate)
        {
            _baseUrl = baseUrl;
            var apiRoot = new Uri(baseUrl, segmentTemplate);
            _InitalPosition = apiRoot.Segments.Length - 1;
        }

        public ApiRouter(string segmentTemplate) 
        {
            _segmentTemplate = segmentTemplate;
            _MatchPattern = CreateMatchPattern(segmentTemplate);
            
        }

        public ApiRouter RegisterLink<TLink, TController>() {
            _Links.Add(typeof(TLink),typeof(TController));
            return this;
        }

        public IDictionary<string,ApiRouter> ChildRouters { get { return _childRouters; }
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

        public ApiRouter WithConstraint(string parameterName, IHttpRouteConstraint constraint)
        {
            _Constraints.Add(parameterName, constraint);
            return this;
        }
        public ApiRouter WithConstraint(string parameterName, string regexConstraint)
        {
            _Constraints.Add(parameterName,new RegexConstraint(regexConstraint));
            
            return this;
        }

        public ApiRouter WithHandler(DelegatingHandler delegatingHandler)
        {
            // router -> MessageHandler -> router ->Child Router
            _MessageHandler = delegatingHandler;
            _MessageHandler.InnerHandler = this;
            InnerHandler = _MessageHandler; // This creates a crazy loop. The router has to know when it is re-entered to prevent the cyle.
                                            // Current innerhandler is a HttpControllerDispatcher which we aren't using, so throw away :-)
            
            
            return this;
        }


        public ApiRouter Add(string childSegmentTemplate, Action<ApiRouter> configure)
        {
            var childrouter = new ApiRouter(childSegmentTemplate);
            configure(childrouter);
            return Add(childrouter);
        }

        public ApiRouter Add(ApiRouter childRouter)
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
                pathRouteData = RetrieveRouteData(request);

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
                return base.SendAsync(request, cancellationToken);  // Call message handler, which will call back into here.
            }

            pathRouteData.AddRouter(this); // Track ApiRouters used to resolve path

            if (pathRouteData.EndOfPath())
            {
                if (_HasController)
                {
                    return Dispatch(request, cancellationToken);
                }
                else
                {
                    var tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.SetResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                    return tcs.Task;
                }
            } else
            {
                pathRouteData.MoveToNext();

                var nextRouter = GetMatchingRouter(request, pathRouteData.CurrentSegment);

                if (nextRouter != null)
                {
                    return nextRouter.SendAsync(request, cancellationToken);
                }

                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                return tcs.Task;

                
            }

        }

        internal ApiRouter GetMatchingRouter(HttpRequestMessage message, string template)
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

        private PathRouteData RetrieveRouteData(HttpRequestMessage request)
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
                    pathRouteData = new PathRouteData(request.RequestUri, _InitalPosition);

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
                pathRouteData = new PathRouteData(request.RequestUri, _InitalPosition);
                request.Properties.Add(new KeyValuePair<string, object>(HttpPropertyKeys.HttpRouteDataKey, pathRouteData));
            }


            // Copy over values provided during Router setup
            if (_Defaults != null)
            {
                foreach (var value in _Defaults)
                {
                    pathRouteData.Values[value.Key] =  value.Value;
                }
            }
            return pathRouteData;
        }

        private IHttpRouteData GetRouteData(HttpRequestMessage request)
        {
            return (IHttpRouteData)request.Properties[HttpPropertyKeys.HttpRouteDataKey];
        }

        public bool Matches(HttpRequestMessage request, string paramTemplate)
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

            foreach (var constraint in _Constraints)
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


            IHttpControllerActivator activator = configuration.Services.GetHttpControllerActivator();
            var httpController = activator.Create(request, controllerDescriptor, _ControllerType);

            var controllerContext = new HttpControllerContext(configuration, GetRouteData(request), request);
            controllerContext.ControllerDescriptor = controllerDescriptor;
            controllerContext.Controller = httpController;

            return httpController.ExecuteAsync(controllerContext, cancellationToken);

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


        public ApiRouter To<T>(string instance = null)
        {
            _ControllerInstanceName = instance;
            _ControllerType = typeof(T);
            _HasController = true;
            return this;
        }
        public ApiRouter To<T>(object routeValues, string instance = null)
        {
            _Defaults = new HttpRouteValueDictionary(routeValues);

            To<T>(instance);
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

        public Uri GetUrlForController(Type type, string instance = null)
        {
            var leafRouter = FindControllerRouter(type, instance);
            if (leafRouter == null) return null;

            return GetUrlFromRouter(leafRouter);
        }

        public Uri GetUrlFromRouter(ApiRouter router) {
            string url = router.SegmentTemplate ;
            while (router.ParentRouter != null)
            {
                router = router.ParentRouter;
                url = router.SegmentTemplate + @"/" + url;
            }
            return new Uri(_baseUrl,url);
        }


        public ApiRouter FindControllerRouter(Type type, string instance = null)
        {

            if (_HasController && _ControllerType == type && _ControllerInstanceName == instance) return this;

            foreach (var childRouter in ChildRouters.Values)
            {
                var router = childRouter.FindControllerRouter(type, instance);
                if (router != null) return router;
            }
            return null;
        }

        IHttpRouteData IHttpRoute.GetRouteData(string virtualPathRoot, HttpRequestMessage request) {
            throw new NotImplementedException();        
        }

        IHttpVirtualPathData IHttpRoute.GetVirtualPath(HttpControllerContext controllerContext, IDictionary<string, object> values) {
            return new HttpVirtualPathData(this, GetUrlFromRouter(this).OriginalString);
        }

        string IHttpRoute.RouteTemplate {
            get { return _segmentTemplate; }
        }

        IDictionary<string, object> IHttpRoute.Defaults {
            get { throw new NotImplementedException(); }
        }

        IDictionary<string, object> IHttpRoute.Constraints {
            get { throw new NotImplementedException(); }
        }

        IDictionary<string, object> IHttpRoute.DataTokens {
            get { throw new NotImplementedException(); }
        }

        public Link GetLink<TLink>(HttpRequestMessage requestMessage)
        {
            var link = GetLink<TLink>();
            var routeData = requestMessage.GetRouteData();
            foreach(var value in  routeData.Values)
            {
                link.SetParameter(value.Key,value.Value);
            }
            return link;
        }

        public Link GetLink<TLink>() {
            var controllerType = (from lk in _Links where lk.Key == typeof (TLink) select lk.Value).FirstOrDefault();
            var link = (Link)Activator.CreateInstance(typeof (TLink));
            link.Target = RootRouter.GetUrlForController(controllerType);
            return link;
        }

        private ApiRouter RootRouter
        {
            get
            {
                ApiRouter target = this;
                while (target.ParentRouter != null)
                {
                    target = target.ParentRouter;
                }
                return target;
            }
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