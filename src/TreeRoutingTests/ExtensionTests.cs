using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tavis;
using Xunit;

namespace TreeRoutingTests
{
    public class ExtensionTests
    {

        [Fact]
        public void BuildTreeFromPath()
        {
            var router = new TreeRoute("");

            router.AddWithPath("foo/bar/baz", r => r.To<FakeController>());
            router.AddWithPath("foo/bar", r => r.To<Fake2Controller>());
            router.AddWithPath("foo/bar/baz/blur", r => r.To<Fake3Controller>());


            var httpRequestMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/foo/bar/baz")
            };

            var httpRequestMessage2 = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/foo/bar")
            };

            var httpRequestMessage3 = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost/foo/bar/baz/blur")
            };
            
            var routeData = router.GetRouteData("/", httpRequestMessage);
            var routeData2 = router.GetRouteData("/", httpRequestMessage2);
            var routeData3 = router.GetRouteData("/", httpRequestMessage3);

            Assert.Equal("Fake",routeData.Values["controller"]);
            Assert.Equal("Fake2", routeData2.Values["controller"]);
            Assert.Equal("Fake3", routeData3.Values["controller"]);

        }

    }
}
