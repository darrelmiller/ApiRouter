# Api Router #

This message handler is an alternative to using MVC Routes in a Web API project to route requests to controllers.

Routers are created at service initialization time in a hierarchy that matches the URI space of the API and controllers are attached to routers in the hierarchy.


## Update: ##

With version 0.1.8 there is now a new class TreeRoute that implements IHttpRoute so it can be used alongside regular ASP.NET Routing.  There is no need to for a messagehandler any more.

Basic usage:

You can build the tree manually in a similar way you build XDocuments, e.g.

       var treeRoute = new TreeRoute("api",
                                     new TreeRoute("Contact",
                                                   new TreeRoute("{id}").To<FakeController>()));

to use the route, simply add it to your route collection.

       config.Routes.Add("mytreeroute",treeRoute);


You can also build the trees in a way that is similar to regular routes.  e.g.

            var route = new TreeRoute("games");
            route.AddWithPath("{gametitle}/Setup/{gamesid}", r => r.To<SetupController>());
            route.AddWithPath("{gametitle}/Resources/{resourcetype}/{resourceid}", r => r.To<ResourceController>());
            route.AddWithPath("{gametitle}/{gameid}/Chat/{chatid}", r => r.To<ChatController>()); 
            route.AddWithPath("{gametitle}/{gameid}/State/{stateid}", r => r.To<StateController>());

Behind the scenes we do actually build a tree of TreeRoutes based on the paths provided.

One advantage of building a tree of routes is that for large and deep trees, the performance of route resolution should be significantly better.  Also, future versions will allow attaching message handlers to the tree and routing resolution will generate a custom pipeline of message handlers that are specific to that route.

Currently, RouteConstraints and RouteDefaults are not implemented, neither is URL generation implemented in a way that ASP.NET can use it. All of this should be possible, it will just take time.

Feedback is welcome. 

Basic usage:

To handle the root path `/` you just need to create a router and connect a controller to it.
```c#
	new ApiRouter("").To<RootController>();
```

To create a route with a child path segment, simply call Add with a child router. e.g.
```c# 
	new ApiRouter("")
		.Add(new ApiRouter("search").To<SearchController>());
```

This approach works but matching parenthesis can be come a pain with large hierarchies so there is an overload that takes the segment template and a lambda that will configure the child ApiRouter.
```c#
	new ApiRouter("")
		.Add("search", childrouter => childrouter.To<SearchController>());
```

This will handle requests to `/search` to the SearchController.


To use the router, simply add it to the MessageHandlers collection.  Here is how you would do it in a Self Host scenario: 
```c#
    static void Main(string[] args)
    {
        var baseurl = new Uri("http://localhost:9000/");
        var config = new HttpSelfHostConfiguration(baseurl);

        var router = new ApiRouter("foo");
        config.MessageHandlers.Add(router);

        var host = new HttpSelfHostServer(config);
        host.OpenAsync().Wait();

        Console.WriteLine("Host open.  Hit enter to exit...");

        Console.Read();

        host.CloseAsync().Wait();
    }
```

With Web Host you need at least one MVC route to cause the Message handler pipeline to be processed.  Check out the sample to see how it is done.

For a more complex example, see the GitHubApiRouter class.