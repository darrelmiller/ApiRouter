# Api Router #

This message handler is an an alternative to using MVC Routes in a Web API project to route requests to controllers.

Routers are created at service initialization time in a hierarchy that matches the URI space of the API and controllers are attached to routers in the hierarchy.  


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