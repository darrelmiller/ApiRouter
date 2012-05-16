using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConferenceApi.Lookups;
using ConferenceApi.Lookups.Rooms;
using ConferenceApi.Persons;
using ConferenceApi.Sessions;
using ConferenceApi.Sessions.AttendanceLinks;
using RoutingMessageHandlerConsole.Lookups.Speakers;
using RoutingMessageHandlerConsole.Lookups.Tracks;
using Tavis;

namespace ConferenceApi
{
    public class ConferenceApiRouter : ApiRouter
    {
        public ConferenceApiRouter() : base("")
        {
            Add
                    (new ApiRouter("api").DispatchTo<RootController>()  // the registered IHttpControllerActivator to construct the controller
                        .Add(new ApiRouter("Lookups")
                            .Add(new ApiRouter("Rooms").DispatchTo<RoomsController>())
                            .Add(new ApiRouter("Tracks").DispatchTo<TracksController>())
                            .Add(new ApiRouter("Speakers").DispatchTo<SpeakersController>())
                            )

                         .Add(new ApiRouter("Sessions").DispatchTo<SessionsController>(new { sessionid = 99 })
                                        .WithHandler(new LoggingHandler())      // Any delegating handler can be attached to any Router in the hierarchy
                // and will be applied to all requests dispatched to it or a child router

                                .Add(new ApiRouter("briefs").DispatchTo<SessionsController>(new { Style = "briefs" })  // Any default values can be passed to the DispatchTo.  
                // These are added to the standard MVC RouteData object
                                .Add(new ApiRouter("speakers").DispatchTo<SessionsController>(new { Style = "speakers" }))
                                .Add(new ApiRouter("taggroups").DispatchTo<SessionsController>(new { Style = "taggroups" })))
                                .Add(new ApiRouter("{sessionid}")
                                            .WithConstraint("sessionid", @"\d+")                // Parameters Constraints can be added. A string is converted to a RegexConstraint
                // Any IHttpRouteConstraint can be added
                                            .DispatchTo<SessionsController>(new { sessionid = 0 })   // Default parameter values can be provided
                                    .Add(new ApiRouter("attendancelinks").DispatchTo<AttendanceLinksController>()))   // Sub resources can be mapped to their own controller or their parents
                              )

                        .Add(new ApiRouter("attendancelinks").DispatchTo<AttendanceLinksController>()
                                        .Add(new ApiRouter("{pid},{sid}")                               // segment templates are regexs that can contain any combination of parameters and literals
                                                        .DispatchTo<AttendanceLinksController>(new { action = "GetLink" })  // The action parameter can be specified to disambiguate requests on controllers
                                                        .WithConstraint("pid", @"\d+")
                                                        .WithConstraint("sid", @"\d+")
                                             ))

                        .Add(new ApiRouter("Persons").DispatchTo<PersonsController>()
                                .Add(new ApiRouter("{personid}").WithConstraint("personid", @"\d+").DispatchTo<SessionsController>())
                        ));
        }
    }

 
}
