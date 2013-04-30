using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace RoutingMessageHandlerConsole.Controllers
{
    public class HomeController : ApiController
    {
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage() {Content = new StringContent("Home")};
        }


     }


    public class Movie
    {
        
    }
}
