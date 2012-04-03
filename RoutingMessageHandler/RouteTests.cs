using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace RoutingMessageHandler
{
    public class RouteTests
    {

        [Fact]
        public void RouteRootPath()
        {
            var root = new Router("").AttachController(() => new FakeController("Root"));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost").Result;

            Assert.True(FakeController.WasInstantiated);
            Assert.Equal("Root",FakeController.ControllerId);
        }

        [Fact]
        public void RouteStaticPath()
        {
            var root = new Router("")
                        .Add(new Router("Desktop").AttachController(() => new FakeController("Desktop")));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Desktop").Result;

            Assert.NotNull(response);
            Assert.Equal("Desktop", FakeController.ControllerId);
        }

        [Fact]
        public void RouteWithinLargeStaticPath()
        {
            var root = new Router("")
                        .Add(new Router("Mobile").AttachController(() => new FakeController("Mobile")))
                        .Add(new Router("Admin")
                                .Add(new Router("Backups"))
                                .Add(new Router("Hotfixes").AttachController(() => new FakeController("Hotfixes"))))
                        .Add(new Router("Desktop").AttachController(() => new FakeController("Desktop")));

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
                                .Add(new Router("{id}").AttachController(() => new FakeController("Contact"))));

            var httpClient = new HttpClient(root);

            var response = httpClient.GetAsync("http://localhost/Contact/23").Result;

            var pathparser = (PathParser)response.RequestMessage.Properties["PathParser"];
            Assert.Equal("23", pathparser.GetParameter("id"));
            Assert.Equal("Contact", FakeController.ControllerId);
        }

        [Fact]
        public void PathParserWithEmptyUri()
        {
            var pathParser = new PathParser(new Uri("http://localhost"));
            Assert.True(pathParser.EndOfPath());
        }


        [Fact]
        public void PathParserShouldInitializeWithRootAsFirstSegment()
        {
            var pathParser = new PathParser(new Uri("http://localhost/Desktop"));
            Assert.Equal("",pathParser.CurrentSegment);
        }

        [Fact]
        public void PathParserCanWalk()
        {
            var pathParser = new PathParser(new Uri("http://localhost/foo"));
            pathParser.MoveToNext();
            Assert.Equal("foo", pathParser.CurrentSegment);
            Assert.True(pathParser.EndOfPath());
        }

    }
}
