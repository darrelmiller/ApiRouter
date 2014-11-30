using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Tavis;

namespace ApiRouterTests.Discovery.AreaA.SubAreaAB
{
    [PathParameter("{id}", false)]
    public class ChildABController : ApiController
    {
            public HttpResponseMessage Get(int id = 0)
            {
                return new HttpResponseMessage() { Content = new StringContent("SubAreaAB Child " + id.ToString()) };
            }
        
    }
}
