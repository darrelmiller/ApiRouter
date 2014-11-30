using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http.Routing;

namespace Tavis
{
    public class TreeRouteData : IHttpRouteData
    {
        private readonly Uri _requestUri;
        private string[] _Segments;
        private int _Position;

        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        private readonly List<TreeRoute> _SegmentRoutes = new List<TreeRoute>();  // List of nodes from Leaf to root 

        private List<HttpMessageHandler> _messageHandlers = new List<HttpMessageHandler>();  // MessageHandlers identified in the traversal from root to leaf


        public TreeRoute RootRouter { get { return _SegmentRoutes[0]; } }

        public void AddRouter(TreeRoute router)
        {
            _SegmentRoutes.Add(router);
        }


        public TreeRouteData(Uri requestUri, int initialPosition)
        {
            _requestUri = requestUri;
            _Segments = _requestUri.Segments;
            _Position = initialPosition;

        }

        public IHttpRoute Route { get { return _SegmentRoutes.LastOrDefault(); } }  // Leaf node

        public bool EndOfPath()
        {
            return _Position == (_Segments.LongCount() - 1);
        }

        public bool AtRoot()
        {
            return _Position ==_Segments.LongCount();
        }

        public string CurrentSegment
        {
            get
            {
                if (_Position < _Segments.Length)
                {
                    return _Segments[_Position].Replace("/", "");
                }
                return null;
            }
        }

        public IDictionary<string, object> Values
        {
            get { return _values; }
        }

        public List<HttpMessageHandler> MessageHandlers
        {
            get { return _messageHandlers; }
        }

        public void MoveToNext()
        {
            _Position++;
        }

        public void SetParameter(string name, string value)
        {
            Values[name] = value;
        }
        public object GetParameter(string name)
        {
            return Values[name];
        }
    }

}
