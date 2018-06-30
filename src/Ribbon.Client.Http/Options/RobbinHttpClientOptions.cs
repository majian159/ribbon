using Ribbon.LoadBalancer;
using System.Net.Http;

namespace Ribbon.Client.Http
{
    public class RobbinHttpClientOptions
    {
        public HttpClient HttpClient { get; set; }
        public ILoadBalancer LoadBalancer { get; set; }
        public IRetryHandler RetryHandler { get; set; }
    }
}