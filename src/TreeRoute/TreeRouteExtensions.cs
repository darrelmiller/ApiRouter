using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis;

namespace Tavis
{
    
        public static class TreeRouteExtensions
        {

            public static TreeRoute Configure(this TreeRoute router, Action<TreeRoute> configureAction)
            {
                configureAction(router);
                return router;
            }

            public static TreeRoute ToAction(this TreeRoute router, string actionName)
            {
                router.Defaults.Add("action",actionName);
                return router;
            }
            public static TreeRoute AddWithPath(this TreeRoute router, string path, Action<TreeRoute> configure)
            {

                if (path.StartsWith("/")) path = path.Remove(0, 1);
                var segments = path.Split('/');
                var currentRouter = router;
                foreach (var segment in segments)
                {
                    var childRouter = currentRouter.ChildRouters.FirstOrDefault(r => r.SegmentTemplate == segment);
                    if (childRouter != null)
                    {
                        currentRouter = childRouter;
                    }
                    else
                    {
                        var newRouter = new TreeRoute(segment);
                        
                        currentRouter.AddChildRoute(newRouter);
                        currentRouter = newRouter;
                    }
                }
                configure(currentRouter);
                return router;
            }

            public static TreeRoute GetAtPath(this TreeRoute router, string path)
            {

                var segments = path.Split('/');
                var currentRouter = router;
                foreach (var segment in segments)
                {
                    var childRouter = currentRouter.ChildRouters.FirstOrDefault(r => r.SegmentTemplate == segment);
                    if (childRouter != null)
                    {
                        currentRouter = childRouter;
                    }
                    else
                    {
                        return null;
                    }
                }
                return currentRouter;
            }

            public static Uri GetUrl(this TreeRoute router)
            {
                
                string url = router.SegmentTemplate;
                while (router.ParentRouter != null)
                {
                    router = router.ParentRouter;
                    url = router.SegmentTemplate + @"/" + url;
                }
                return new Uri(url, UriKind.Relative);
            }


            public static Uri GetUrlForController(this TreeRoute router, Type type, string instance = null)
            {
                var leafRouter = router.FindControllerRouter(type, instance);
                if (leafRouter == null) return null;

                return leafRouter.GetUrl();
            }


        }
    }

