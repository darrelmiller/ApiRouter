using System;
using System.Linq;

namespace Tavis {
    public static class ApiRouterExtensions {

        public static  ApiRouter AddWithPath(this ApiRouter router, string path, Action<ApiRouter> configure) {

            if (path.StartsWith("/")) path = path.Remove(0, 1);
            var segments = path.Split('/');
            var currentRouter = router;
            foreach (var segment in segments) {
                if (currentRouter.ChildRouters.ContainsKey(segment))
                {
                    currentRouter = currentRouter.ChildRouters[segment];
                } else {
                    currentRouter.Add(segment, (r) => { 
                                                    currentRouter = r;
                                               });
                }
            }
            configure(currentRouter);
            return router;
        }

        public static ApiRouter GetAtPath(this ApiRouter router, string path)
        {

            var segments = path.Split('/');
            var currentRouter = router;
            foreach (var segment in segments)
            {
                if (currentRouter.ChildRouters.ContainsKey(segment))
                {
                    currentRouter = currentRouter.ChildRouters[segment];
                }
                else
                {
                    return null;
                }
            }
            return currentRouter;
        }
    }
}