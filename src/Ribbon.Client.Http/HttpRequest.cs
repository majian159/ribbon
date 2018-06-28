using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Ribbon.Client.Http
{
    public class HttpRequest : ClientRequest
    {
        public HttpRequest(Uri uri)
        {
            Uri = uri;
            Method=HttpMethod.Get;
        }

        public HttpMethod Method { get; set; }
        public HttpHeaders HttpHeaders { get; set; }
        public HttpContent Content { get; set; }
    }
}