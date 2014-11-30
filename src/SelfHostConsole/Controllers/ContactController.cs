using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace RoutingMessageHandlerConsole.Controllers
{
    public class ContactController : ApiController
    {

        public HttpResponseMessage Get(int id)
        {
            return new HttpResponseMessage() { Content = new StringContent("Contact : " + id) };
        }
    }
}
