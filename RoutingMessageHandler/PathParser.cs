using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoutingMessageHandler
{
    public class PathParser
    {
        private readonly Uri _requestUri;
        private string[] _Segments;
        private int _Position;
        private Dictionary<string, string> _Parameters = new Dictionary<string, string>();

        public PathParser(Uri requestUri)
        {
            _requestUri = requestUri;
            _Segments = _requestUri.Segments;
            _Position = 0;
    
        }

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

        internal void MoveToNext()
        {
            _Position++;
        }

        public void AddParameter(string name, string value)
        {
            _Parameters.Add(name, value);
        }
        public string GetParameter(string name)
        {
            return _Parameters[name];
        }
    }


    public class PathWalker
    {

        private List<String> _Segments;
        private int _Position;
        public PathWalker(string path)
        {
            _Segments = new List<string>(path.Split('/'));
            Position = 0;
        }

        public string ProcessedPath
        {
            get
            {
                return String.Join("/", _Segments.Take(Position).ToArray());
            }
        }
        public string UnprocessedPath
        {
            get
            {
                return String.Join("/", _Segments.Skip(Position).ToArray());
            }
        }

        public bool IsLastPosition()
        {
            return _Position == (_Segments.LongCount() - 1);
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
                return _Segments[_Position];
            }
        }
    }
}
