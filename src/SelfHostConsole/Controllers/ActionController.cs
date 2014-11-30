using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace RoutingMessageHandlerConsole.Controllers
{
    public class ActionController : ApiController
    {
        [ActionName("A")]
        public HttpResponseMessage GetA()
        {
            return new HttpResponseMessage();
        }

        [ActionName("B")]
        public HttpResponseMessage GetB()
        {
            return new HttpResponseMessage();
        }
    }
}
