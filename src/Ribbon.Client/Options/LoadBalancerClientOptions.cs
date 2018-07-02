using Ribbon.LoadBalancer;
using System;

namespace Ribbon.Client
{
    public class LoadBalancerClientOptions
    {
        public ILoadBalancer LoadBalancer { get; set; }
        public IRetryHandler RetryHandler { get; set; }
        public Func<string, IServiceProvider, IClient> Creator { get; set; }
        public ExecuteOptions DefaultExecuteOptions { get; set; }
    }
}