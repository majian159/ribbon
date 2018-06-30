using Microsoft.Extensions.Options;
using Ribbon.Client.Impl;
using Ribbon.LoadBalancer;
using Ribbon.LoadBalancer.Impl;
using System;

namespace Ribbon.Client
{
    public class LoadBalancerClientOptionsSetup : IConfigureNamedOptions<LoadBalancerClientOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerOptions> _loadBalancerOptionsMonitor;
        private readonly IOptionsMonitor<LoadBalancerSettings> _loadBalancerSettingsMonitor;
        private readonly IOptionsMonitor<RetryHandlerOptions> _retryHandlerOptionsMonitor;

        public LoadBalancerClientOptionsSetup(IOptionsMonitor<LoadBalancerOptions> loadBalancerOptionsMonitor, IOptionsMonitor<LoadBalancerSettings> loadBalancerSettingsMonitor, IOptionsMonitor<RetryHandlerOptions> retryHandlerOptionsMonitor)
        {
            _loadBalancerOptionsMonitor = loadBalancerOptionsMonitor;
            _loadBalancerSettingsMonitor = loadBalancerSettingsMonitor;
            _retryHandlerOptionsMonitor = retryHandlerOptionsMonitor;
        }

        #region Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void Configure(LoadBalancerClientOptions options)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        #region Implementation of IConfigureNamedOptions<in ClientOptions>

        /// <inheritdoc/>
        public void Configure(string name, LoadBalancerClientOptions options)
        {
            var loadBalancerOptions = _loadBalancerOptionsMonitor.Get(name);
            var loadBalancerSettings = _loadBalancerSettingsMonitor.Get(name);

            var loadBalancer = new DefaultLoadBalancer(loadBalancerOptions.Rule, loadBalancerOptions.Ping, loadBalancerOptions.ServerList, loadBalancerSettings);

            options.LoadBalancer = loadBalancer;
            loadBalancerOptions.Rule.LoadBalancer = loadBalancer;

            var retryHandlerOptions = _retryHandlerOptionsMonitor.Get(name);
            options.RetryHandler = new DefaultLoadBalancerRetryHandler(retryHandlerOptions);
        }

        #endregion Implementation of IConfigureNamedOptions<in ClientOptions>
    }
}