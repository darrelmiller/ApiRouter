using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using Xunit;

namespace RoutingMessageHandler
{
    public class RouteTests
    {

        [Fact]
        public void RouteRootPath()
        {
            var root = new Router("").DispatchTo<FakeController>(new {ControllerId = "Root"});

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost").Result;

            Assert.True(FakeController.WasInstantiated);
            Assert.Equal("Root",FakeController.ControllerId);
        }

        [Fact]
        public void RouteStaticPath()
        {
            var root = new Router("")
                        .Add(new Router("Desktop").DispatchTo<FakeController>(new {ControllerId = "Desktop"}));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Desktop").Result;

            Assert.NotNull(response);
            Assert.Equal("Desktop", FakeController.ControllerId);
        }

        [Fact]
        public void RouteWithinLargeStaticPath()
        {
            var root = new Router("")
                        .Add(new Router("Mobile").DispatchTo<FakeController>(new {ControllerId = "Mobile"}))
                        .Add(new Router("Admin")
                                .Add(new Router("Backups"))
                                .Add(new Router("Hotfixes").DispatchTo<FakeController>(new {ControllerId = "Hotfixes"})))
                        .Add(new Router("Desktop").DispatchTo<FakeController>(new {ControllerId = "Desktop"}));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Admin/Hotfixes").Result;

            Assert.NotNull(response);
            Assert.True(FakeController.WasInstantiated);
            Assert.Equal("Hotfixes", FakeController.ControllerId);
        }

        [Fact]
        public void RouteWithParameterSegement()
        {
            var root = new Router("")
                        .Add(new Router("Contact")
                                .Add(new Router("{id}").DispatchTo<FakeController>(new {ControllerId = "Contact"})));

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
            
            var root = new Router("").WithHandler(fakeHandler).DispatchTo<FakeController>();
                        
            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/").Result;

            Assert.True(fakeHandler.WasCalled);

        }

        [Fact]
        public void SegementWithTwoParameters()
        {
            var root = new Router("foo{a} and {b}");

            var match = root.Matches(null, "foo21 and boo");
            
            Assert.True(match);
        }


        [Fact]
        public void PathParserWithEmptyUri()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost"));
            Assert.True(pathParser.EndOfPath());
        }


        [Fact]
        public void PathParserShouldInitializeWithRootAsFirstSegment()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost/Desktop"));
            Assert.Equal("",pathParser.CurrentSegment);
        }

        [Fact]
        public void PathParserCanWalk()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost/foo"));
            pathParser.MoveToNext();
            Assert.Equal("foo", pathParser.CurrentSegment);
            Assert.True(pathParser.EndOfPath());
        }

    }

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
