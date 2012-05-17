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
    public class RouteTests
    {

        [Fact]
        public void RouteRootPath()
        {
            var root = new ApiRouter("").To<FakeController>(new {ControllerId = "Root"});

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost").Result;

            Assert.True(FakeController.WasInstantiated);
            Assert.Equal("Root",FakeController.ControllerId);
        }

        [Fact]
        public void RouteStaticPath()
        {
            var root = new ApiRouter("")
                        .Add(new ApiRouter("Desktop").To<FakeController>(new {ControllerId = "Desktop"}));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Desktop").Result;

            Assert.NotNull(response);
            Assert.Equal("Desktop", FakeController.ControllerId);
        }

        [Fact]
        public void RouteWithinLargeStaticPath()
        {
            var root = new ApiRouter("")
                        .Add(new ApiRouter("Mobile").To<FakeController>(new {ControllerId = "Mobile"}))
                        .Add(new ApiRouter("Admin")
                                .Add(new ApiRouter("Backups"))
                                .Add(new ApiRouter("Hotfixes").To<FakeController>(new {ControllerId = "Hotfixes"})))
                        .Add(new ApiRouter("Desktop").To<FakeController>(new {ControllerId = "Desktop"}));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Admin/Hotfixes").Result;

            Assert.NotNull(response);
            Assert.True(FakeController.WasInstantiated);
            Assert.Equal("Hotfixes", FakeController.ControllerId);
        }

        [Fact]
        public void RouteWithParameterSegement()
        {
            var root = new ApiRouter("")
                        .Add(new ApiRouter("Contact")
                                .Add(new ApiRouter("{id}").To<FakeController>(new {ControllerId = "Contact"})));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Contact/23").Result;

            var pathparser = (PathRouteData)response.RequestMessage.Properties[HttpPropertyKeys.HttpRouteDataKey];
            Assert.Equal("23", pathparser.GetParameter("id"));
            Assert.Equal("Contact", FakeController.ControllerId);
        }


        [Fact]
        public void RouteWithMessageHandler()
        {
            var fakeHandler = new FakeHandler();
            
            var root = new ApiRouter("").WithHandler(fakeHandler).To<FakeController>();
                        
            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/").Result;

            Assert.True(fakeHandler.WasCalled);

        }

        [Fact]
        public void SegementWithTwoParameters()
        {
            var root = new ApiRouter("foo{a} and {b}");

            var match = root.Matches(null, "foo21 and boo");
            
            Assert.True(match);
        }


        [Fact]
        public void PathParserWithEmptyUri()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost"),0);
            Assert.True(pathParser.EndOfPath());
        }


        [Fact]
        public void PathParserShouldInitializeWithRootAsFirstSegment()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost/Desktop"), 0);
            Assert.Equal("",pathParser.CurrentSegment);
        }

        [Fact]
        public void PathParserCanWalk()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost/foo"), 0);
            pathParser.MoveToNext();
            Assert.Equal("foo", pathParser.CurrentSegment);
            Assert.True(pathParser.EndOfPath());
        }


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
            var router = new ApiRouter("foo",new Uri("http://localhost/blah/")).To<FakeController>();

            var httpClient = new HttpClient(router);

            var response = httpClient.GetAsync("http://localhost/blah/foo").Result;

            Assert.True(FakeController.WasInstantiated);
        }

        [Fact]
        public void GraftRouterAtRoot()
        {
            var router = new ApiRouter("", new Uri("http://localhost/")).To<FakeController>();

            var httpClient = new HttpClient(router);

            var response = httpClient.GetAsync("http://localhost/").Result;

            Assert.True(FakeController.WasInstantiated);
        }

        [Fact]
        public void GraftRouterAtRootB()
        {
            var router = new ApiRouter("", new Uri("http://localhost/api/")).To<FakeController>();

            var httpClient = new HttpClient(router);

            var response = httpClient.GetAsync("http://localhost/api").Result;

            Assert.True(FakeController.WasInstantiated);
        }


        [Fact]
        public void GetUrlOfController()
        {
            var router = new ApiRouter("foo", new Uri("http://localhost/api/")).To<FakeController>();


            var url = router.GetUrlForController(typeof (FakeController));

            Assert.Equal("http://localhost/api/foo", url.AbsoluteUri);
        }
    }



    public class SetupController: ApiController { }
    public class ResourceController : ApiController { }
    public class ChatController : ApiController { }
    public class StateController : ApiController { }

    internal class FakeHandler : DelegatingHandler
    {
        public bool WasCalled { get; set; }
        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            WasCalled = true;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
