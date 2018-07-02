using System.Net.Http;

namespace Ribbon.Client.Http
{
    public class RibbonHttpClientOptions
    {
        public HttpClient HttpClient { get; set; }
        public LoadBalancerClientOptions LoadBalancerClientOptions { get; set; }
    }
}