using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Tavis;
using Xunit;

namespace ApiRouterTests
{
    public class LinkTests
    {


        [Fact]
        public void GetUrlOfController()
        {
            var router = new ApiRouter("foo", new Uri("http://localhost/api/")).To<FakeController>();


            var url = router.GetUrlForController(typeof (FakeController));

            Assert.Equal("http://localhost/api/foo", url.AbsoluteUri);
        }

        [Fact]
        public void GetLinkByTypeBetweenRouters() {
            var router = new ApiRouter("foo", new Uri("http://localhost/api/")).To<FakeController>()
                    .Add("child", cr => {
                                      cr.To<FakeChildController>();
                                      cr.RegisterLink<ParentLink, FakeController>();
                                  });


            var link = router.ChildRouters["child"].GetLink<ParentLink>();

            Assert.Equal("http://localhost/api/foo", link.Target.AbsoluteUri);
            Assert.IsType(typeof (ParentLink), link);
        }


        [Fact]
        public void UseRouteDataToResolveUriTemplate()
        {
            var routeData = new PathRouteData(new Uri("http://foo"), 0);

            routeData.Values.Add("foo","1");
            routeData.Values.Add("bar", "2");
            routeData.Values.Add("baz", 3);
            var link = new Link() {Target = new Uri("http://localhost/{?foo,bar,baz}")};
            foreach (var value in routeData.Values)
            {
                link.SetParameter(value.Key,value.Value);    
            }
            var request = link.CreateRequest();

            Assert.Equal("http://localhost/?foo=1&bar=2&baz=3", request.RequestUri.AbsoluteUri);
            
        }


        [Fact]
        public void UseUriParamToResolveLink()
        {
            var router = new ApiRouter("", new Uri("http://localhost/"))
                            .Add("Customer", rc => rc 
                                .Add("{customerid}",rci => rci.To<CustomerController>()
                                                    .RegisterLink<InvoicesLink,InvoicesController>()
                                                    .Add("Invoices",ri => ri.To<InvoicesController>())
                                    )
                                )
                            .Add("Invoice", ri => ri
                                    .Add("{invoiceid}", rii => rii.To<InvoiceController>()
                                                        .RegisterLink<CustomerLink,CustomerController>()                    
                                    )
                                );

            var httpClient = new HttpClient(new FakeServer(router));

            var response = httpClient.GetAsync("http://localhost/Customer/23/").Result;

            

            Assert.Equal("http://localhost/Customer/23/Invoices", response.Content.ReadAsStringAsync().Result);

        }

    }

    public class InvoicesLink : Link { }
    public class CustomerLink : Link { }

    public class CustomerController : ApiController
    {
        public HttpResponseMessage Get ()
        {
            var router = Request.GetRouteData().Route as ApiRouter;
            var url = router.GetLink<InvoicesLink>(Request).GetResolvedTarget(); //Need to add the ability to resolve templates for serialization.
            return new HttpResponseMessage()
                       {
                           Content = new StringContent(url.AbsoluteUri)
                       };
        }
    }

    
    public class ParentLink : Link {}
    public class FakeChildController : FakeController {}


    public class InvoicesController : ApiController
    {

    }
    public class InvoiceController : ApiController
    {

    }

}
