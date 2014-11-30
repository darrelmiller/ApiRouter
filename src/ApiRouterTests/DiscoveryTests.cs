using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Tavis;
using Xunit;

namespace ApiRouterTests
{
    public class DiscoveryTests
    {
        private ApiRouter _router;
        private HttpClient _httpClient;

        public DiscoveryTests() {
            _router = RouteDiscovery.Search(typeof(Discovery.RootController), new Uri("http://localhost/"));

            _httpClient = new HttpClient(new FakeServer(_router));
        }


        [Fact]
        public void WhenRequestingRootThenRootShouldRespond()
        {

            var response = _httpClient.GetAsync("http://localhost/").Result;

            Assert.Equal("RootController", response.Content.ReadAsStringAsync().Result);
        }

      

        [Fact]
        public void WhenRequestingOtherChildOfRootThenOtherChildOfRootShouldRespond()
        {

            var response = _httpClient.GetAsync("http://localhost/OtherChild").Result;

            Assert.Equal("OtherChildController", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void WhenRequestingChildOfRootThenChildOfRootShouldRespond()
        {

            var response = _httpClient.GetAsync("http://localhost/Child").Result;

            Assert.Equal("0", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void WhenRequestingChildWithParameterTheValueShouldBePassed()
        {

            var response = _httpClient.GetAsync("http://localhost/Child/23").Result;

            Assert.Equal("23", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ChildControllerInArea()
        {

            var response = _httpClient.GetAsync("http://localhost/AreaA/Child").Result;

            Assert.Equal("AreaA Child", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void ChildControllerInSubArea()
        {

            var response = _httpClient.GetAsync("http://localhost/AreaA/SubAreaAB/ChildAB/75").Result;

            Assert.Equal("SubAreaAB Child 75", response.Content.ReadAsStringAsync().Result);
        }
        [Fact]
        public void ChildControllerInSubAreaWithoutParam()
        {

            var response = _httpClient.GetAsync("http://localhost/AreaA/SubAreaAB/ChildAB").Result;

            Assert.Equal("SubAreaAB Child 0", response.Content.ReadAsStringAsync().Result);
        }

        
    }
}
