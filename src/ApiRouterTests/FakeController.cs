using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace ApiRouterTests
{
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

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            var httpRouteData = request.GetRouteData();
            if (httpRouteData.Values.ContainsKey("ControllerId"))
            {
                ControllerId = (string) httpRouteData.Values["ControllerId"];
            }
            return new HttpResponseMessage() {RequestMessage = request};
        }

        public Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            return new TaskFactory<HttpResponseMessage>().StartNew(() => Get(controllerContext.Request));
        }
    }
}