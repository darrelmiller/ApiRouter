using System;
using System.Net.Http;
using System.Web.Http;
using RoutingMessageHandler;

namespace ConferenceApi.Sessions
{


    public class SessionsController : ApiController
    {
        


        public SessionsController()
        {
            
        }


        public HttpResponseMessage Get(int? sessionid)
        {
            var httpRouteData = Request.GetRouteData();

            object style = null;
            if (httpRouteData.Values.ContainsKey("Style"))
            {
                style = httpRouteData.Values["Style"];
            }
            return new HttpResponseMessage()
            {
                Content = new StringContent(String.Format("Here's a sessions resource with an id: {0} and style : {1}", sessionid, style))
            };
        }

        public static Router GetRouter()
        {
            throw new NotImplementedException();
        }
    }
}
