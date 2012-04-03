using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;

namespace RoutingMessageHandler
{
    public class Router : DelegatingHandler
    {
        private readonly string _segmentTemplate;
        private Dictionary<string, Router> _childRouters = new Dictionary<string, Router>();
        private Dictionary<string, Router> _childParameterRouters = new Dictionary<string, Router>();
        private bool _hasController;
        private Func<object> _ControllerFactory;
        public Router ParentRouter { get; set; }



        public Router(string segmentTemplate)
        {
            _segmentTemplate = segmentTemplate;
        }

        
        public string SegmentTemplate
        {
            get { return _segmentTemplate; }
        }

        
        public Router Add(Router childRouter)
        {
            if (childRouter.SegmentTemplate.Contains("{"))
            {
                _childParameterRouters.Add(childRouter.SegmentTemplate, childRouter);
            }
            else
            {
                _childRouters.Add(childRouter.SegmentTemplate, childRouter);
            }
            childRouter.ParentRouter = this;    
            return this;
        }
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            PathParser pathParser;
            if (request.Properties.ContainsKey("PathParser"))
            {
                pathParser = (PathParser)request.Properties["PathParser"];
            }
            else
            {
                pathParser = new PathParser(request.RequestUri);
                request.Properties.Add(new KeyValuePair<string, object>("PathParser", pathParser));
            }

            // If this is a parameter based segment router, then parse out the parameter
            if (_segmentTemplate.Contains("{"))
            {
                pathParser.AddParameter(_segmentTemplate.Replace("{", "").Replace("}", ""), pathParser.CurrentSegment);
            }

            // TODO Process added MessageHandlers


            if (pathParser.EndOfPath())
            {
                if (_hasController)
                {
                    return new TaskFactory().StartNew<HttpResponseMessage>(() => Dispatch(request));
                }
                else
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }
            } else
            {
                pathParser.MoveToNext();
                if (_childRouters.ContainsKey(pathParser.CurrentSegment))
                {
                    return _childRouters[pathParser.CurrentSegment].SendAsync(request, cancellationToken);
                }
                else
                {
                    var paramRouter =
                        (_childParameterRouters.Values.Where(r => r.Matches(pathParser.CurrentSegment)).FirstOrDefault());
                    if (paramRouter != null)
                    {
                        return paramRouter.SendAsync(request, cancellationToken);
                    } else
                    {

                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                }
            }

            

            
        }

        private bool Matches(string paramTemplate)
        {
            return true;
        }

        private HttpResponseMessage Dispatch(HttpRequestMessage request)
        {
            var controller = _ControllerFactory();
            var method = controller.GetType().GetMethod("Get");
            return (HttpResponseMessage)method.Invoke(controller, new object[] { request });
        }

        internal Router AttachController(Func<object> controllerFactory)
        {
            _ControllerFactory = controllerFactory;
            _hasController = true;
            return this;
        }
    }
}