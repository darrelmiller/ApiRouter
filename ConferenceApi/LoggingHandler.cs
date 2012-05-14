using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ConferenceApi
{
    public class LoggingHandler : DelegatingHandler
    {

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Debug.WriteLine(request.RequestUri.AbsoluteUri);
            return base.SendAsync(request, cancellationToken);
        }
    }
}
