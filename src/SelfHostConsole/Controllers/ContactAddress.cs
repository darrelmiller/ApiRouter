using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace RoutingMessageHandlerConsole.Controllers
{
    public class ContactAddressController : ApiController
    {

        public HttpResponseMessage Get(int id, int addressid)
        {
            return new HttpResponseMessage() { Content = new StringContent("Contact : " + id + " address : " + addressid) };
        }
    }
}
