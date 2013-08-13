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
            var root = new ApiRouter("").To<FakeController>(new { ControllerId = "Root" });

            var httpClient = new HttpClient(new FakeServer(root));

            var response = httpClient.GetAsync("http://localhost").Result;

            Assert.True(FakeController.WasInstantiated);
            Assert.Equal("Root", FakeController.ControllerId);
        }


      

        [Fact]
        public void RouteStaticPath()
        {
            var root = new ApiRouter("")
                        .Add(new ApiRouter("Desktop").To<FakeController>(new { ControllerId = "Desktop" }));

            var httpClient = new HttpClient(new FakeServer(root));

            var response = httpClient.GetAsync("http://localhost/Desktop").Result;

            Assert.NotNull(response);
            Assert.Equal("Desktop", FakeController.ControllerId);
        }

        [Fact]
        public void RouteWithinLargeStaticPath()
        {
            var root = new ApiRouter("")
                        .Add(new ApiRouter("Mobile").To<FakeController>(new { ControllerId = "Mobile" }))
                        .Add(new ApiRouter("Admin")
                                .Add(new ApiRouter("Backups"))
                                .Add(new ApiRouter("Hotfixes").To<FakeController>(new { ControllerId = "Hotfixes" })))
                        .Add(new ApiRouter("Desktop").To<FakeController>(new { ControllerId = "Desktop" }));

            var httpClient = new HttpClient(new FakeServer(root));

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
                                .Add(new ApiRouter("{id}").To<FakeController>(new { ControllerId = "Contact" })));

            var httpClient = new HttpClient(new FakeServer(root));

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

            var httpClient = new HttpClient(new FakeServer(root));

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
            var pathParser = new PathRouteData(new Uri("http://localhost"), 0);
            Assert.True(pathParser.EndOfPath());
        }


        [Fact]
        public void PathParserShouldInitializeWithRootAsFirstSegment()
        {
            var pathParser = new PathRouteData(new Uri("http://localhost/Desktop"), 0);
            Assert.Equal("", pathParser.CurrentSegment);
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
        public void CanFindApiRouteBeforeStandardRoute()
        {
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "TreeApi",
                routeTemplate: "tree/{*path}",
                defaults: new { ControllerId = "CanFindApiRouteBeforeStandardRoute" },
                constraints: null,
                handler: new ApiRouter("~/tree").To<FakeController>());

            config.Routes.MapHttpRoute(
                            name: "DefaultApi",
                            routeTemplate: "api/{controller}/{id}",
                            defaults: new { id = RouteParameter.Optional }
                        );

            var server = new HttpServer(config);
            var client = new HttpClient(server);
            var response = client.GetAsync("http://localhost:64921/tree").Result;

            Assert.NotNull(response);
            Assert.Equal("CanFindApiRouteBeforeStandardRoute", FakeController.ControllerId);
        }

        [Fact]
        public void CanFindStandardRouteAfterApiRoute()
        {
            var config = new HttpConfiguration();            

            config.Routes.MapHttpRoute(
                name: "TreeApi",
                routeTemplate: "api/tree/{*path}",
                defaults: null,
                constraints: null,
                handler: new ApiRouter("tree").To<FakeController>());

            config.Routes.MapHttpRoute(
                            name: "DefaultApi",
                            routeTemplate: "api/{controller}/{id}",
                            defaults: new { 
                                id = RouteParameter.Optional, 
                                ControllerId = "CanFindStandardRouteAfterApiRoute" 
                            }
                        );            

            var server = new HttpServer(config);
            var client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/api/fake").Result;

            Assert.NotNull(response);
            Assert.Equal("CanFindStandardRouteAfterApiRoute", FakeController.ControllerId);           
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
}
