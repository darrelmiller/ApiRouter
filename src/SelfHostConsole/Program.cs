using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.Http.SelfHost;
using GitHubApi;
using RoutingMessageHandlerConsole.Controllers;
using Tavis;

namespace RoutingMessageHandlerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseurl = new Uri("http://localhost:9000/");
            var config = new HttpSelfHostConfiguration(baseurl);

            //config.MessageHandlers.Add(new GitHubApiRouter(baseurl));
            
            //config.Routes.Add("default", new TreeRoute("",new TreeRoute("home").To<HomeController>(), 
            //                                              new TreeRoute("contact",
            //                                                        new TreeRoute("{id}",
            //                                                            new TreeRoute("address", 
            //                                                                 new TreeRoute("{addressid}").To<ContactAddressController>())
            //                                                        ).To<ContactController>())
            //                                              )
            //                  );

            var route = new TreeRoute("api");
            route.AddWithPath("home", r => r.To<HomeController>());
            route.AddWithPath("contact/{id}",r => r.To<ContactController>());
            route.AddWithPath("contact/{id}/adddress/{addressid}", r => r.To<ContactAddressController>());

            config.Routes.Add("default", route);


            var host = new HttpSelfHostServer(config);
            host.OpenAsync().Wait();

            Console.WriteLine("Host open.  Hit enter to exit...");

            Console.Read();

            host.CloseAsync().Wait();
        }
    }


  }

