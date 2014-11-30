using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Tavis;
using Xunit;

namespace TreeRoutingTests
{
    public class RouteTests
    {

        [Fact]
        public void RouteRootPathWithTree()
        {
            var root = new TreeRoute("").To<FakeController>();


            var routeData = root.GetRouteData("", new HttpRequestMessage() { RequestUri = new Uri("http://example.org/") });

            Assert.Equal("Fake", routeData.Values["controller"]);
        }

        [Fact]
        public void TreeRouteWithParameterSegement()
        {
            var treeRoute = new TreeRoute("",
                                     new TreeRoute("Contact",
                                                   new TreeRoute("{id}").To<FakeController>()));


            var routeData = treeRoute.GetRouteData("", new HttpRequestMessage() { RequestUri = new Uri("http://example.org/Contact/24") });
            Assert.Equal("Fake", routeData.Values["controller"]);
            Assert.Equal("24", routeData.Values["id"]);

        }


        [Fact]
        public void TreeRoutesWithActions()
        {
            var treeRoute = new TreeRoute("",
                                     new TreeRoute("A").Configure(r => r.Defaults.Add("action","A")).To<ActionApiController>());


            var routeData = treeRoute.GetRouteData("", new HttpRequestMessage() { RequestUri = new Uri("http://example.org/A") });
            Assert.Equal("ActionApi", routeData.Values["controller"]);
            Assert.Equal("A", routeData.Values["action"]);

        }

        [Fact]
        public void RouteWithinLargeStaticPathUsingTreeAndLinqStyleInit()
        {
            var treeRoute = new TreeRoute("",
                                new TreeRoute("Mobile").To<FakeController>(),
                                new TreeRoute("Admin",
                                    new TreeRoute("Backups"),
                                    new TreeRoute("Hotfixes").To<Fake2Controller>()
                                    ),
                                new TreeRoute("Desktop").To<Fake3Controller>()
                              );

            var routeData = treeRoute.GetRouteData("", new HttpRequestMessage() { RequestUri = new Uri("http://example.org/Mobile") });
            Assert.Equal("Fake", routeData.Values["controller"]);

            var routeData2 = treeRoute.GetRouteData("", new HttpRequestMessage() { RequestUri = new Uri("http://example.org/Admin/Hotfixes") });
            Assert.Equal("Fake2", routeData2.Values["controller"]);

            var routeData3 = treeRoute.GetRouteData("", new HttpRequestMessage() { RequestUri = new Uri("http://example.org/Admin") });
            Assert.Null(routeData3);


        }

        [Fact]
        public void GraftRouterAtSubTree()
        {
            var router = new TreeRoute("foo").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/blah/foo")
            };

            routeData = router.GetRouteData("/blah/", httpRequestMessage);

            Assert.Equal("Fake", routeData.Values["controller"]);
        }


        [Fact]
        public void GraftRouterAtRoot()
        {
            var router = new TreeRoute("").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/")
            };

            routeData = router.GetRouteData("/", httpRequestMessage);

            Assert.Equal("Fake", routeData.Values["controller"]);
        }

        [Fact]
        public void GraftRouterAtVirtualPath()
        {
            var router = new TreeRoute("").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/mywebapp")
            };

            routeData = router.GetRouteData("/mywebapp/", httpRequestMessage);

            Assert.Equal("Fake", routeData.Values["controller"]);
        }


        [Fact]
        public void GraftRouterAtRootNotFound()
        {
            var router = new TreeRoute("").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/blah")
            };

            routeData = router.GetRouteData("/", httpRequestMessage);

            Assert.Null(routeData);
        }

        [Fact]
        public void GraftRouterAtApi()
        {
            var router = new TreeRoute("api").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/api/")
            };

            routeData = router.GetRouteData("/", httpRequestMessage);

            Assert.Equal("Fake", routeData.Values["controller"]);
        }

        [Fact]
        public void GraftRouterAtApiNotFound()
        {
            var router = new TreeRoute("api").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/apix/")
            };

            routeData = router.GetRouteData("/", httpRequestMessage);

            Assert.Null(routeData);
        }
        [Fact]
        public void GraftRouterAtApiNotFoundTooLong()
        {
            var router = new TreeRoute("api").To<FakeController>();

            IHttpRouteData routeData = null;
            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/api/foo")
            };

            routeData = router.GetRouteData("/", httpRequestMessage);

            Assert.Null(routeData);
        }


        [Fact]
        public void gamesRoutes2()
        {
            var route = new TreeRoute("games");
            route.AddWithPath("{gametitle}/Setup/{gamesid}", r => r.To<SetupController>());
            route.AddWithPath("{gametitle}/Resources/{resourcetype}/{resourceid}", r => r.To<ResourceController>());
            route.AddWithPath("{gametitle}/{gameid}/Chat/{chatid}", r => r.To<ChatController>()); 
            route.AddWithPath("{gametitle}/{gameid}/State/{stateid}", r => r.To<StateController>());

            var getRouteData = route.GetRouteData("/", new HttpRequestMessage() { RequestUri = new Uri("http://localhost/games/GodsOfWar/277/Chat/177") });

            var url = route.GetUrlForController(typeof(ChatController));

            Assert.Equal("Chat", getRouteData.Values["controller"]);
            Assert.Equal("GodsOfWar", getRouteData.Values["gametitle"]);
            Assert.Equal("277", getRouteData.Values["gameid"]);
            Assert.Equal("177", getRouteData.Values["chatid"]);
            Assert.Equal("games/{gametitle}/{gameid}/Chat/{chatid}", url.OriginalString);
        }

        [Fact]
        public void gamesRoutes3()
        {
            var router = new TreeRoute("games");
            router.AddWithPath("{gametitle}/Setup/{gamesid}", apiRouter => apiRouter.To<SetupController>());
            router.AddWithPath("{gametitle}/Resources/{resourcetype}/{resourceid}", apiRouter => apiRouter.To<ResourceController>());
            router.AddWithPath("{gametitle}/{gameid}/{controller}/{id}", r => { }); 
            
            var getRouteData = router.GetRouteData("/", new HttpRequestMessage() { RequestUri = new Uri("http://localhost/games/GodsOfWar/277/Chat/177") });

            
            Assert.Equal("Chat", getRouteData.Values["controller"]);
            Assert.Equal("GodsOfWar", getRouteData.Values["gametitle"]);
            Assert.Equal("277", getRouteData.Values["gameid"]);
            Assert.Equal("177", getRouteData.Values["id"]);
            
        }


        [Fact]
        public void DefaultWebApiRoute()
        {
            var route = new TreeRoute("api");
             route.AddWithPath("{controller}/{id}", r => { });

            var getRouteData = route.GetRouteData("/", new HttpRequestMessage() { RequestUri = new Uri("http://localhost/api/contact/23") });

            Assert.Equal("contact", getRouteData.Values["controller"]);
            Assert.Equal("23", getRouteData.Values["id"]);
        }



        [Fact]
        public void SetupBigRouteTable()
        {
            var route = new TreeRoute("");
            route.AddWithPath("x/foo/{controller}/{id}", r => { });
            route.AddWithPath("x/bar/{controller}/{id}", r => { });
            route.AddWithPath("x/baz/{controller}/{id}", r => { });
            route.AddWithPath("y/blip/{controller}/{id}", r => { });
            route.AddWithPath("y/flid/{controller}/{id}", r => { });
            route.AddWithPath("y/flod/{controller}/{id}", r => { });


            var routes = new HttpRouteCollection();
            routes.MapHttpRoute("a", "x/foo/{controller}/{id}");
            routes.MapHttpRoute("b", "x/bar/{controller}/{id}");
            routes.MapHttpRoute("c", "x/baz/{controller}/{id}");
            routes.MapHttpRoute("d", "y/blip/{controller}/{id}");
            routes.MapHttpRoute("e", "y/flid/{controller}/{id}");
            routes.MapHttpRoute("f", "y/flod/{controller}/{id}");

            var httpRequestMessage = new HttpRequestMessage() { RequestUri = new Uri("http://localhost/y/flid/boo/22") };

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 1000000; i++)
            {
                var getRouteData = route.GetRouteData("/", httpRequestMessage);    
            }
            
            stopwatch.Stop();
            Debug.WriteLine("That took " + stopwatch.ElapsedMilliseconds);
            
            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < 1000000; i++)
            {
                var mvcroute = routes.GetRouteData(httpRequestMessage);
            }
            stopwatch.Stop();
            Debug.WriteLine("That took " + stopwatch.ElapsedMilliseconds);                       
        }

        [Fact]
        public void RequestToRootWillPassThrough()
        {
            var router = new TreeRoute("api").To<FakeController>();
            
            var httpRequestMessage = new HttpRequestMessage()
                {
                    RequestUri = new Uri("http://localhost:2112/")
                };

            var routeData = router.GetRouteData("/", httpRequestMessage);

            Assert.Null(routeData);
        }
    }

    public class SetupController : ApiController { }
    public class ResourceController : ApiController { }
    public class ChatController : ApiController { }
    public class StateController : ApiController { }

}
