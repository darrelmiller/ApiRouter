using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using ConferenceApi;
using ConferenceApi.Lookups.Rooms;
using ConferenceApi.Persons;
using ConferenceApi.Sessions;
using ConferenceApi.Sessions.AttendanceLinks;
using RoutingMessageHandler;

using RoutingMessageHandlerConsole.Lookups.Speakers;
using RoutingMessageHandlerConsole.Lookups.Tracks;

namespace RoutingMessageHandlerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseurl = new Uri("http://localhost:9000/");
            var config = new HttpSelfHostConfiguration(baseurl);

            var router = new Router("api").DispatchTo<RootController>()
                        .Add(new Router("Lookups")
                            .Add(new Router("Rooms").DispatchTo<RoomsController>())
                            .Add(new Router("Tracks").DispatchTo<TracksController>())
                            .Add(new Router("Speakers").DispatchTo<SpeakersController>())
                            )
                         .Add(new Router("Sessions").DispatchTo<SessionsController>()
                                .Add(new Router("briefs").DispatchTo<SessionsController>(new { Style = "briefs" })
                                .Add(new Router("speakers").DispatchTo<SessionsController>(new { Style = "speakers" }))
                                .Add(new Router("taggroups").DispatchTo<SessionsController>(new { Style = "taggroups" })))
                                .Add(new Router("{id}").DispatchTo<SessionsController>())
                                    .Add(new Router("attendancelinks").DispatchTo<AttendanceLinksController>())
                             )
                        .Add(new Router("Persons").DispatchTo<PersonsController>());

            config.MessageHandlers.Add(router);

            var host = new HttpSelfHostServer(config);
            host.OpenAsync().Wait();

            Console.WriteLine("Host open.  Hit enter to exit...");

            Console.Read();

            host.CloseAsync().Wait();
        }
    }
}

