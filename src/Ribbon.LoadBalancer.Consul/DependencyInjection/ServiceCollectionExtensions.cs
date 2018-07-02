using Microsoft.Extensions.DependencyInjection;
using Ribbon.Client;

namespace Ribbon.LoadBalancer.Consul
{
    public static class ServiceCollectionExtensions
    {
        public static IRibbonBuilder AddConsulDiscovery(this IRibbonBuilder builder)
        {
            builder
                .Services
                .ConfigureOptions<ConsulLoadBalancerOptionsSetup>();

            return builder;
        }
    }
}