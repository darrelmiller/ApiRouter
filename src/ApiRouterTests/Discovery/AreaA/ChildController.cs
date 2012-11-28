using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ApiRouterTests.Discovery.AreaA
{
    public class ChildController : ApiController
    {

        public HttpResponseMessage Get() {
            return new HttpResponseMessage() {Content = new StringContent("AreaA Child")};
        }
    }
}
