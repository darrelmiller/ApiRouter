using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Routing;

namespace RoutingMessageHandler
{
    public class PathRouteData : IHttpRouteData
    {
        private readonly Uri _requestUri;
        private string[] _Segments;
        private int _Position;
        private Dictionary<string, object> _values = new Dictionary<string, object>();
        private IHttpRoute _Route;  // PathRoute

        public PathRouteData(Uri requestUri)
        {
            _requestUri = requestUri;
            _Segments = _requestUri.Segments;
            _Position = 0;
    
        }

        public IHttpRoute Route { get { return _Route; } }

        public bool EndOfPath()
        {
            return _Position == (_Segments.LongCount()-1);
        }

        public int Position
        {
            get { return _Position; }
            set { _Position = value; }
        }

        public string CurrentSegment
        {
            get
            {
                return _Segments[_Position].Replace("/","");
            }
        }

        public IDictionary<string, object> Values
        {
            get { return _values; }
        }

        internal void MoveToNext()
        {
            _Position++;
        }

        public void SetParameter(string name, string value)
        {
            Values[name] =  value;
        }
        public object GetParameter(string name)
        {
            return Values[name];
        }
    }


    
}
