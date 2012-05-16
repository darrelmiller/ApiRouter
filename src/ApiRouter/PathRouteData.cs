using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Routing;

namespace Tavis
{
    public class PathRouteData : IHttpRouteData
    {
        private readonly Uri _requestUri;
        private string[] _Segments;
        private int _Position;
        private Dictionary<string, object> _values = new Dictionary<string, object>();

        public PathRouteData(Uri requestUri)
        {
            _requestUri = requestUri;
            _Segments = _requestUri.Segments;
            _Position = 0;
    
        }

        public IHttpRoute Route { get { return null; } }

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

        public void MoveToNext()
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
