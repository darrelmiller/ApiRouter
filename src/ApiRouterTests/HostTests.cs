using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using Tavis;
using Xunit;

namespace ApiRouterTests
{
    public class HostTests
    {

        [Fact]
        public void gamesRoutes()
        {
            var router =
                new ApiRouter("games", new Uri("http://localhost/"))
                    .Add("{gametitle}", rg => rg
                             .Add("Setup", rs => rs.Add("{gameid}", rgi => rgi.To<SetupController>()))
                             .Add("Resources", rr => rr.Add("{resourcetype}", rt => rt.Add("{resourceId}", ri => ri.To<ResourceController>())))
                             .Add("{gameid}", rgi => rgi
                                      .Add("Chat", rc => rc.Add("{chatid}", rci => rci.To<ChatController>()))
                                      .Add("State", rs => rs.Add("{stateid}", rsi => rsi.To<StateController>()))
                             ));

            var url = router.GetUrlForController(typeof(ChatController));

            Assert.Equal("http://localhost/games/{gametitle}/{gameid}/Chat/{chatid}", url.OriginalString);
        }




        [Fact]
        public void GraftRouterAtSubTree()
        {
            var router = new ApiRouter("foo", new Uri("http://localhost/blah/")).To<FakeController>();

            var httpClient = new HttpClient(new FakeServer(router));

            var response = httpClient.GetAsync("http://localhost/blah/foo").Result;

            Assert.True(FakeController.WasInstantiated);
        }

        [Fact]
        public void GraftRouterAtRoot()
        {
            var router = new ApiRouter("", new Uri("http://localhost/")).To<FakeController>();

            var httpClient = new HttpClient(new FakeServer(router));

            var response = httpClient.GetAsync("http://localhost/").Result;

            Assert.True(FakeController.WasInstantiated);
        }

        [Fact]
        public void GraftRouterAtRootB()
        {
            var router = new ApiRouter("", new Uri("http://localhost/api/")).To<FakeController>();

            var httpClient = new HttpClient(new FakeServer(router));

            var response = httpClient.GetAsync("http://localhost/api").Result;

            Assert.True(FakeController.WasInstantiated);
        }




        public class SetupController : ApiController { }
        public class ResourceController : ApiController { }
        public class ChatController : ApiController { }
        public class StateController : ApiController { }

    }

    public class FakeServer : DelegatingHandler
    {
        public FakeServer(ApiRouter router)
        {
            InnerHandler = router;

        }
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var httpConfiguration = new HttpConfiguration();
         //   httpConfiguration.Services.Add(typeof(IHttpControllerActivator), new TestControllerActivator());
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = httpConfiguration;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
