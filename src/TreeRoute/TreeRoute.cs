using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http.Routing;

namespace Tavis
{
    public class TreeRoute : IHttpRoute
    {
        private readonly string _segmentTemplate;
        private readonly List<TreeRoute> _childRouters = new List<TreeRoute>();
        
        private bool _HasController;
        private Type _ControllerType;
        private readonly Regex _MatchPattern;


        public bool UseRegex { get; set; }
        public static bool CaseSensitive = true;
        public TreeRoute ParentRouter { get; set; }

        private DelegatingHandler _MessageHandler;
        
        private  int _InitalPosition = 0;
        private string _ControllerInstanceName;


        public TreeRoute(string segmentTemplate, params TreeRoute[] childroutes)
        {
            Defaults = new Dictionary<string, object>();
            Constraints = new Dictionary<string, object>();
            DataTokens = new Dictionary<string, object>();

            _segmentTemplate = segmentTemplate;
            UseRegex = _segmentTemplate.Contains("{");

            if (UseRegex) _MatchPattern = CreateMatchPattern(segmentTemplate,CaseSensitive);

            if (childroutes != null)
            {
                foreach (var childroute in childroutes)
                {
                    AddChildRoute(childroute);
                }
            }

        }
        public void AddChildRoute(TreeRoute treeRoute)
        {
            treeRoute.ParentRouter = this;
            if (!treeRoute.UseRegex)  // Place static matches first before wildcard matches
            {
                _childRouters.Insert(0, treeRoute);
            }
            else
            {
                _childRouters.Add(treeRoute);
            }
        }
        public IEnumerable<TreeRoute> ChildRouters
        {
            get { return _childRouters; }
        }
        
        public string SegmentTemplate
        {
            get { return _segmentTemplate; }
        }

        public TreeRoute WithHandler(DelegatingHandler delegatingHandler)
        {
            _MessageHandler = delegatingHandler;

            return this;
        }


        public TreeRoute To<T>(string instance = null)
        {
            To(typeof(T), instance);
            return this;
        }

        internal TreeRoute To(Type type, string instance = null)
        {
            _ControllerInstanceName = instance;
            _ControllerType = type;
            _HasController = true;
            return this;
        }
      

        public TreeRoute FindControllerRouter(Type type, string instance = null)
        {

            if (_HasController && _ControllerType == type && _ControllerInstanceName == instance) return this;

            foreach (var childRouter in ChildRouters)
            {
                var router = childRouter.FindControllerRouter(type, instance);
                if (router != null) return router;
            }
            return null;
        }

        private bool Matches(HttpRequestMessage request, string paramTemplate)
        {
            if (!UseRegex) return String.Equals(paramTemplate,_segmentTemplate,StringComparison.InvariantCulture);

            var result = _MatchPattern.Match(paramTemplate);
            if (!result.Success)
                return false;

            var parameterValues = new Dictionary<string, object>();
            for (int i = 1; i < result.Groups.Count; i++)
            {
                var name = _MatchPattern.GroupNameFromNumber(i);
                var value = result.Groups[i].Value;
                parameterValues.Add(name, value);
            }

            return true;
        }

        private IHttpRouteData Find(HttpRequestMessage request, TreeRouteData treeRouteData)
        {
            if (treeRouteData == null)
            {
                treeRouteData = new TreeRouteData(request.RequestUri, _InitalPosition);
                

                if (SegmentTemplate == string.Empty)
                {
                    if (treeRouteData.AtRoot())
                    {
                        return Dispatch(treeRouteData); //Special case root
                    } 
                }
                else
                {
                    if (!Matches(request, treeRouteData.CurrentSegment)) return null; // URI segment does not match this TreeRoute
                }
            }

          

            ProcessTreeRoute(treeRouteData);

            if (treeRouteData.EndOfPath())
            {
                return Dispatch(treeRouteData);
            }

            treeRouteData.MoveToNext();

            var nextRouter = _childRouters.FirstOrDefault(childRouter => childRouter.Matches(request, treeRouteData.CurrentSegment));

            if (nextRouter != null)
            {
                return nextRouter.Find(request, treeRouteData);
            }

            return null;
        }

        private IHttpRouteData Dispatch(TreeRouteData treeRouteData)
        {
            if (_HasController)
            {
                treeRouteData.Values["controller"] = _ControllerType.Name.Replace("Controller", "");
                return treeRouteData;
            } else if (treeRouteData.Values.ContainsKey("controller"))
            {
                return treeRouteData;
            } 
            return null;
        }

        private void ProcessTreeRoute(TreeRouteData treeRouteData)
        {
            if (_MessageHandler != null) treeRouteData.MessageHandlers.Add(_MessageHandler);

            foreach (var value in Defaults)
            {
                treeRouteData.Values[value.Key] = value.Value;
            }

            // Parse Parameters from URL and add them to the treeRouteData
            if (UseRegex)
            {
                var result = _MatchPattern.Match(treeRouteData.CurrentSegment);

                for (int i = 1; i < result.Groups.Count; i++)
                {
                    var name = _MatchPattern.GroupNameFromNumber(i);
                    var value = result.Groups[i].Value;
                    treeRouteData.SetParameter(name, value);
                }
            }


            treeRouteData.AddRouter(this); // Track ApiRouters used to resolve path
        }

        private static Regex CreateMatchPattern(string segmentTemplate, bool caseSensitive)
        {
            var pattern = new StringBuilder();
            pattern.Append("^");
            StringBuilder paramName = null;
            foreach (char tcharacter in segmentTemplate)
            {

                if (tcharacter == '{')
                {
                    paramName = new StringBuilder();
                }

                if (tcharacter != '}' && tcharacter != '{')
                {
                    if (paramName == null)
                    {
                        pattern.Append(tcharacter);
                    }
                    else
                    {
                        paramName.Append(tcharacter);
                    }
                }
                if (tcharacter == '}')
                {

                    pattern.Append("(?<" + paramName.ToString() + ">.*)");
                    paramName = null;
                }
            }
            pattern.Append("$");

            var regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (caseSensitive) regexOptions |= RegexOptions.IgnoreCase;
            return new Regex(pattern.ToString(), regexOptions);
        }


        public IHttpRouteData GetRouteData(string virtualPathRoot, HttpRequestMessage request)
        {
            if (!String.IsNullOrEmpty(virtualPathRoot))
            {
                
                var segments = virtualPathRoot.Split('/');
                if (string.IsNullOrEmpty(SegmentTemplate))
                {
                    _InitalPosition = segments.Length-2;  // this Route represents the "/" 
                }
                else
                {
                    _InitalPosition = segments.Length - 1;
                }
                
            }

            return Find(request, null);
        }


        public IHttpVirtualPathData GetVirtualPath(HttpRequestMessage request, IDictionary<string, object> values)
        {
            // I don't know what this is for yet.
            return null;
        }

        public string RouteTemplate { get; private set; }
        public IDictionary<string, object> Defaults { get; private set; }
        public IDictionary<string, object> Constraints { get; private set; }
        public IDictionary<string, object> DataTokens { get; private set; }
        public HttpMessageHandler Handler
        {
            get { return _MessageHandler; }  //Not sure whether this is good or we should get it from the TreeRouteData
        }

    }

   
}
