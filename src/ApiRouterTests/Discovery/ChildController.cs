using System.Net.Http;
using System.Web.Http;
using Tavis;

namespace ApiRouterTests.Discovery
{
    [PathParameter("{id}",false)]
    public class ChildController : ApiController
    {
        public HttpResponseMessage Get(int id = 0) {

            return new HttpResponseMessage() { Content = new StringContent(id.ToString())};
        }


        //[PathMap("All", HttpMethod.Get)]
        //public void GetAll() {
            
        //}
    }
}
