using Microsoft.Extensions.Options;
using Ribbon.Client.Impl;
using Ribbon.LoadBalancer;
using Ribbon.LoadBalancer.Impl;

namespace Ribbon.Client
{
    public class LoadBalancerClientOptionsSetup : IConfigureNamedOptions<LoadBalancerClientOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerOptions> _loadBalancerOptionsMonitor;
        private readonly IOptionsMonitor<RetryHandlerOptions> _retryHandlerOptionsMonitor;

        public LoadBalancerClientOptionsSetup(IOptionsMonitor<LoadBalancerOptions> loadBalancerOptionsMonitor, IOptionsMonitor<RetryHandlerOptions> retryHandlerOptionsMonitor)
        {
            _loadBalancerOptionsMonitor = loadBalancerOptionsMonitor;
            _retryHandlerOptionsMonitor = retryHandlerOptionsMonitor;
        }

        #region Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void Configure(LoadBalancerClientOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        #region Implementation of IConfigureNamedOptions<in ClientOptions>

        /// <inheritdoc/>
        public void Configure(string name, LoadBalancerClientOptions options)
        {
            var loadBalancerOptions = _loadBalancerOptionsMonitor.Get(name);

            var loadBalancer = new DefaultLoadBalancer(loadBalancerOptions);

            options.LoadBalancer = loadBalancer;

            var retryHandlerOptions = _retryHandlerOptionsMonitor.Get(name);
            options.RetryHandler = new DefaultLoadBalancerRetryHandler(retryHandlerOptions);
        }

        #endregion Implementation of IConfigureNamedOptions<in ClientOptions>
    }
}