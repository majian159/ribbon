using Ribbon.LoadBalancer;

namespace Ribbon.Client
{
    public class LoadBalancerClientOptions
    {
        public ILoadBalancer LoadBalancer { get; set; }
        public IRetryHandler RetryHandler { get; set; }
    }
}