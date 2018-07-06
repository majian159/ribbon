using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Consul.Discovery;
using System;

namespace Ribbon.LoadBalancer.ConsulDiscovery
{
    public class ConsulLoadBalancerOptionsSetup : IConfigureNamedOptions<LoadBalancerOptions>
    {
        private readonly IServiceProvider _services;
        private static readonly ConsulPing Ping = new ConsulPing();
        private static HealthServiceServerListFilter _healthServiceServerListFilter;

        public ConsulLoadBalancerOptionsSetup(IServiceProvider services)
        {
            _services = services;
            if (_healthServiceServerListFilter == null)
            {
                _healthServiceServerListFilter = new HealthServiceServerListFilter(services.GetRequiredService<ILogger<HealthServiceServerListFilter>>());
            }
        }

        #region Implementation of IConfigureOptions<in LoadBalancerOptions>

        /// <inheritdoc/>
        public void Configure(LoadBalancerOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in LoadBalancerOptions>

        #region Implementation of IConfigureNamedOptions<in LoadBalancerOptions>

        /// <inheritdoc/>
        public void Configure(string name, LoadBalancerOptions options)
        {
            if (options.ServerList != null)
            {
                return;
            }

            var serverListTypeName = options.Settings.LoadBalancerServerListTypeName;
            var pingTypeName = options.Settings.LoadBalancerPingTypeName;
            var serverListFilterTypeName = options.Settings.ServerListFilterTypeName;

            if (!string.IsNullOrEmpty(serverListTypeName) && Type.GetType(serverListTypeName) != typeof(ConsulServerList))
            {
                return;
            }

            options.ServerList = new ConsulServerList(name, _services.GetService<ConsulClient>(), _services.GetService<IOptions<ConsulDiscoveryOptions>>());

            if (pingTypeName == null || Type.GetType(pingTypeName) == typeof(ConsulPing))
            {
                options.Ping = Ping;
            }

            if (serverListFilterTypeName == null ||
                Type.GetType(serverListFilterTypeName) == typeof(HealthServiceServerListFilter))
            {
                options.ServerListFilter = _healthServiceServerListFilter;
            }

            if (options.ServerListUpdater == null)
            {
                var loadBalancerConfig = _services.GetService<IOptionsMonitor<LoadBalancerConfig>>().Get(name);
                options.ServerListUpdater = new FastConsulServerListUpdater(name, _services, loadBalancerConfig);
            }
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerOptions>
    }
}