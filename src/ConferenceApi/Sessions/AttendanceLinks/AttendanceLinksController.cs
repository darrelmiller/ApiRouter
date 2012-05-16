using System;
using System.Net.Http;
using System.Web.Http;
using Tavis;

namespace ConferenceApi.Sessions.AttendanceLinks
{
    public class AttendanceLinksController : ApiController
    {

        public HttpResponseMessage Get(int? id)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(String.Format("Here's a attendancelinks resource with a session id: {0}", id))
            };
        }

        public HttpResponseMessage GetLink(int? pid, int? sid)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(String.Format("Here's a attendancelinks resource with a pid: {0} and sid: {1} ", pid, sid))
            };
        }

        public static ApiRouter GetRouter()
        {
            throw new NotImplementedException();
        }
    }
}
