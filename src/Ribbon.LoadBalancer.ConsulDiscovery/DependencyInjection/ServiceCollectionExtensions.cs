using Microsoft.Extensions.DependencyInjection;

namespace Ribbon.LoadBalancer.ConsulDiscovery
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