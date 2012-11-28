using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tavis
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PathParameterAttribute : Attribute
    {
        public bool Required { get; private set; }
        public string PathTemplate { get; private set; }

        public PathParameterAttribute(string pathTemplate, bool required) {
            Required = required;
            PathTemplate = pathTemplate;
        }
    }
}
