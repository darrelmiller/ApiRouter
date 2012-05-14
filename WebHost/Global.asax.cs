using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using ConferenceApi;
using ConferenceApi.Lookups;
using ConferenceApi.Lookups.Rooms;
using ConferenceApi.Persons;
using ConferenceApi.Sessions;
using ConferenceApi.Sessions.AttendanceLinks;
using RoutingMessageHandler;
using RoutingMessageHandlerConsole.Lookups.Speakers;
using RoutingMessageHandlerConsole.Lookups.Tracks;

namespace WebHost
{
    public class Global : System.Web.HttpApplication
    {

        void Application_Start(object sender, EventArgs e)
        {
            var router = new Router("").Add
                    (new Router("api").DispatchTo<RootController>()  // the registered IHttpControllerActivator to construct the controller
                        .Add(new Router("Lookups")
                            .Add(new Router("Rooms").DispatchTo<RoomsController>())
                            .Add(new Router("Tracks").DispatchTo<TracksController>())
                            .Add(new Router("Speakers").DispatchTo<SpeakersController>())
                            )

                         .Add(new Router("Sessions").DispatchTo<SessionsController>(new { sessionid = 99 })
                                        .WithHandler(new LoggingHandler())      // Any delegating handler can be attached to any Router in the hierarchy
                                                                                // and will be applied to all requests dispatched to it or a child router

                                .Add(new Router("briefs").DispatchTo<SessionsController>(new {Style="briefs"})  // Any default values can be passed to the DispatchTo.  
                                                                                                                // These are added to the standard MVC RouteData object
                                .Add(new Router("speakers").DispatchTo<SessionsController>(new {Style="speakers"}))
                                .Add(new Router("taggroups").DispatchTo<SessionsController>(new {Style="taggroups"})))
                                .Add(new Router("{sessionid}")
                                            .WithConstraint("sessionid", @"\d+")                // Parameters Constraints can be added. A string is converted to a RegexConstraint
                                                                                                // Any IHttpRouteConstraint can be added
                                            .DispatchTo<SessionsController>(new { sessionid = 0 })   // Default parameter values can be provided
                                    .Add(new Router("attendancelinks").DispatchTo<AttendanceLinksController>()))   // Sub resources can be mapped to their own controller or their parents
                              )

                        .Add(new Router("attendancelinks").DispatchTo<AttendanceLinksController>()
                                        .Add(new Router("{pid},{sid}")                               // segment templates are regexs that can contain any combination of parameters and literals
                                                        .DispatchTo<AttendanceLinksController>(new {action="GetLink"})  // The action parameter can be specified to disambiguate requests on controllers
                                                        .WithConstraint("pid", @"\d+")
                                                        .WithConstraint("sid", @"\d+")
                                             ))     

                        .Add(new Router("Persons").DispatchTo<PersonsController>()
                                .Add(new Router("{personid}").WithConstraint("personid",@"\d+").DispatchTo<SessionsController>())
                        ));


            GlobalConfiguration.Configuration.MessageHandlers.Add(router);
            GlobalConfiguration.Configuration.Routes.MapHttpRoute("api", "api/{*path}");  // Need this one route to get the messagehandlers to kick in.

        }



        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

        }

        void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started

        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
