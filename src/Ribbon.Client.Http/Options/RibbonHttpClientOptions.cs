using Ribbon.LoadBalancer;
using System.Net.Http;

namespace Ribbon.Client.Http
{
    public class RibbonHttpClientOptions
    {
        public HttpClient HttpClient { get; set; }
        public ILoadBalancer LoadBalancer { get; set; }
        public IRetryHandler RetryHandler { get; set; }
    }
}