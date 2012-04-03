using System.Net.Http;

namespace RoutingMessageHandler
{
    public class FakeController
    {
        private static string _controllerId;

        public static string ControllerId { get; set; }

        public static bool WasInstantiated { get; set; }

        public FakeController(string controllerId)
        {
            ControllerId = controllerId;
            WasInstantiated = true;
        }

        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            return new HttpResponseMessage() {RequestMessage = request};
        }
    }
}