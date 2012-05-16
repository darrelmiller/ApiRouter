using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using GitHubApi;

namespace RoutingMessageHandlerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseurl = new Uri("http://localhost:9000/");
            var config = new HttpSelfHostConfiguration(baseurl);

            config.MessageHandlers.Add(new GitHubApiRouter(baseurl));

            var host = new HttpSelfHostServer(config);
            host.OpenAsync().Wait();

            Console.WriteLine("Host open.  Hit enter to exit...");

            Console.Read();

            host.CloseAsync().Wait();
        }
    }
}

