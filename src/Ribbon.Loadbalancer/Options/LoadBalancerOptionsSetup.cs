using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ribbon.LoadBalancer.Impl.Ping;
using Ribbon.LoadBalancer.Impl.Rule;
using Ribbon.LoadBalancer.Impl.ServerList;
using System;
using System.Linq;

namespace Ribbon.LoadBalancer
{
    public class LoadBalancerOptionsSetup : IConfigureNamedOptions<LoadBalancerOptions>, IPostConfigureOptions<LoadBalancerOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerConfig> _settingsMonitor;
        private readonly IServiceProvider _services;

        public LoadBalancerOptionsSetup(IOptionsMonitor<LoadBalancerConfig> settingsMonitor, IServiceProvider services)
        {
            _settingsMonitor = settingsMonitor;
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
            var settings = _settingsMonitor.Get(name);

            if (settings.ListOfServers != null && settings.ListOfServers.Any())
            {
                options.ServerList = new ConfigurationBasedServerList(settings);
            }

            options.Rule = TryGetInstance<IRule>(settings.LoadBalancerRuleTypeName);
            options.ServerList = TryGetInstance<IServerList<Server>>(settings.LoadBalancerServerListTypeName);
            options.Ping = TryGetInstance<IPing>(settings.LoadBalancerPingTypeName);

            options.Settings = settings;
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerOptions>

        #region Implementation of IPostConfigureOptions<in LoadBalancerOptions>

        /// <inheritdoc/>
        public void PostConfigure(string name, LoadBalancerOptions options)
        {
            if (options.Ping == null)
            {
                options.Ping = NoOpPing.Default;
            }

            if (options.Rule == null)
            {
                options.Rule = new RoundRobinRule();
            }

            if (options.ServerList == null)
            {
                var settings = _settingsMonitor.Get(name);
                options.ServerList = new ConfigurationBasedServerList(settings);
            }
        }

        #endregion Implementation of IPostConfigureOptions<in LoadBalancerOptions>

        private T TryGetInstance<T>(string typeName)
        {
            try
            {
                var type = string.IsNullOrEmpty(typeName) ? null : Type.GetType(typeName);
                if (type == null)
                {
                    return default(T);
                }
                return (T)ActivatorUtilities.GetServiceOrCreateInstance(_services, type);
            }
            catch
            {
            }

            return default(T);
        }
    }
}