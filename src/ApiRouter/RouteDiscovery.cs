using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Tavis
{
    public class RouteDiscovery
    {
        private class ControllerEntry {
            public string Path { get; set; }
            public Type ControllerType { get; set; }
        }

        public static ApiRouter Search(Type rootType, Uri baseUri) {
            var typename = GetSegmentFromControllerType(rootType);
            var router = new ApiRouter("", baseUri).To(rootType);

            // Find controllers in the namespaces below type
            var controllers = rootType.Assembly.GetTypes()
                .Where(t => typeof(IHttpController).IsAssignableFrom(t) && t.Namespace.StartsWith(rootType.Namespace) && t != rootType)
                .Select(t => new ControllerEntry() { ControllerType = t, Path = t.Namespace.Replace(rootType.Namespace, "").Replace('.', '/') + "/" + GetSegmentFromControllerType(t) })
                .ToList();

            foreach (var controllerEntry in controllers) {
                Type localController = controllerEntry.ControllerType;
                router.AddWithPath(controllerEntry.Path,
                           (r) => {
                              var parameterAttributes = (PathParameterAttribute[])Attribute.GetCustomAttributes(controllerEntry.ControllerType, typeof (PathParameterAttribute));
                               foreach (var pathParameterAttribute in parameterAttributes) {
                                   r.Add(pathParameterAttribute.PathTemplate, (cr) => cr.To(localController));
                               }

                               // If no path parameters or any are optional then connect the controller to the router
                               if (parameterAttributes.Count() == 0 ||
                                   parameterAttributes.Any(pa => pa.Required == false)) {
                                   r.To(localController);
                               }


                           });


            }
            return router;
        }

        private static string GetSegmentFromControllerType(Type type) {
            return type.Name.Replace("Controller","");
        }
    }
}
