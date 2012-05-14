using System.Net.Http;
using System.Web.Http;

namespace ConferenceApi
{
    public class RootController : ApiController
    {

        

        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage()
                       {
                           Content = new StringContent("Here's a root")
                       };
        }
    }
}
