using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace TreeRoutingTests
{
    public class Fake3Controller : FakeController { }
    public class Fake4Controller : FakeController { }
    public class Fake2Controller : FakeController { }

    public class FakeController : IHttpController
    {
        private static string _controllerId;

        public static string ControllerId { get; set; }

        public static bool WasInstantiated { get; set; }

        public FakeController() : this("")
        {
            
        }

        public FakeController(string controllerId)
        {
            ControllerId = controllerId;
            WasInstantiated = true;
        }

        public virtual HttpResponseMessage Get(HttpRequestMessage request)
        {
            var httpRouteData = request.GetRouteData();
            if (httpRouteData.Values.ContainsKey("ControllerId"))
            {
                ControllerId = (string) httpRouteData.Values["ControllerId"];
            }
            return new HttpResponseMessage() {RequestMessage = request, Content = new StringContent(this.GetType().Name)};
        }

        public Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            return new TaskFactory<HttpResponseMessage>().StartNew(() => Get(controllerContext.Request));
        }
    }

    public class FakeApiController : ApiController
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _getMethod;

        public FakeApiController(Func<HttpRequestMessage, HttpResponseMessage> getMethod)
        {
            _getMethod = getMethod;
        }

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            return _getMethod(request);
        }

        
    }

    public class ActionApiController : ApiController
    {

        [ActionName("A")]
        public HttpResponseMessage GetA(HttpRequestMessage request)
        {
            return new HttpResponseMessage() { Content = new StringContent("A") };
        }

        [ActionName("B")]
        public HttpResponseMessage GetB(HttpRequestMessage request)
        {
            return new HttpResponseMessage() { Content = new StringContent("B") };
        }

        [ActionName("C")]
        public HttpResponseMessage GetC(HttpRequestMessage request)
        {
            return new HttpResponseMessage() { Content = new StringContent("C") };
        }

    }
}