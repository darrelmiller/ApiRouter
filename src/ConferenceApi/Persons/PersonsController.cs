using System;
using System.Net.Http;
using System.Web.Http;
using Tavis;

namespace ConferenceApi.Persons
{
    public class PersonsController : ApiController
    {
      

        public HttpResponseMessage Get(int? personid)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent("Here's a persons resource")
            };
        }

        public static ApiRouter GetRouter()
        {
            throw new NotImplementedException();
        }
    }
}
