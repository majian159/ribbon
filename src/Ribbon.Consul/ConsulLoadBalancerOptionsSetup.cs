using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ribbon.LoadBalancer;
using Steeltoe.Discovery.Consul.Discovery;
using System;

namespace Ribbon.Consul
{
    public class ConsulLoadBalancerOptionsSetup : IConfigureNamedOptions<LoadBalancerOptions>
    {
        private readonly IServiceProvider _services;
        private static readonly ConsulPing Ping = new ConsulPing();

        public ConsulLoadBalancerOptionsSetup(IServiceProvider services)
        {
            _services = services;
        }

        #region Implementation of IConfigureOptions<in LoadBalancerOptions>

        /// <inheritdoc/>
        public void Configure(LoadBalancerOptions options)
        {
            Configure(Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in LoadBalancerOptions>

        #region Implementation of IConfigureNamedOptions<in LoadBalancerOptions>

        /// <inheritdoc/>
        public void Configure(string name, LoadBalancerOptions options)
        {
            options.ServerList = new ConsulServiceList(name, _services.GetService<ConsulClient>(), _services.GetService<IOptionsMonitor<ConsulDiscoveryOptions>>());
            options.Ping = Ping;
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerOptions>
    }
}