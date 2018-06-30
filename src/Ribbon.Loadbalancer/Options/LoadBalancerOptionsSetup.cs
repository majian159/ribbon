using Microsoft.Extensions.Options;
using Ribbon.LoadBalancer.Impl.Ping;
using Ribbon.LoadBalancer.Impl.Rule;
using Ribbon.LoadBalancer.Impl.ServerList;

namespace Ribbon.LoadBalancer
{
    public class LoadBalancerOptionsSetup : IConfigureNamedOptions<LoadBalancerOptions>, IPostConfigureOptions<LoadBalancerOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerConfig> _settingsMonitor;

        public LoadBalancerOptionsSetup(IOptionsMonitor<LoadBalancerConfig> settingsMonitor)
        {
            _settingsMonitor = settingsMonitor;
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

            options.Settings = settings;
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerOptions>

        #region Implementation of IPostConfigureOptions<in LoadBalancerOptions>

        /// <inheritdoc/>
        public void PostConfigure(string name, LoadBalancerOptions options)
        {
            if (options.Ping == null)
            {
                options.Ping = new NoOpPing();
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
    }
}