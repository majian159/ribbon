using Microsoft.Extensions.Options;
using Ribbon.LoadBalancer.Impl.Ping;
using Ribbon.LoadBalancer.Impl.Rule;
using Ribbon.LoadBalancer.Impl.ServerList;

namespace Ribbon.LoadBalancer
{
    public class LoadBalancerOptionsSetup : IConfigureNamedOptions<LoadBalancerOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerSettings> _settingsMonitor;

        public LoadBalancerOptionsSetup(IOptionsMonitor<LoadBalancerSettings> settingsMonitor)
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

            IRule rule;
            IPing ping;
            IServerList<Server> serverList;
            switch (settings.ServerListName)
            {
                default:
                    serverList = new ConfigurationBasedServerList(settings);
                    break;
            }

            switch (settings.RuleName)
            {
                default:
                    rule = new RoundRobinRule();
                    break;
            }

            switch (settings.PingName)
            {
                default:
                    ping = new NoOpPing();
                    break;
            }

            options.Ping = ping;
            options.Rule = rule;
            options.ServerList = serverList;
            options.Settings = settings;
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerOptions>
    }
}