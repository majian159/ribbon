using Microsoft.Extensions.Options;
using Ribbon.Client.Options;

namespace Ribbon.Client.Http.Options
{
    public class HttpLoadBalancerClientOptionsSetup : IConfigureNamedOptions<LoadBalancerClientOptions>
    {
        private readonly IOptionsMonitor<RetryHandlerConfig> _retryHandlerOptionsMonitor;

        public HttpLoadBalancerClientOptionsSetup(IOptionsMonitor<RetryHandlerConfig> retryHandlerOptionsMonitor, IOptionsMonitor<LoadBalancerClientConfig> loadBalancerClientConfigMonitor)
        {
            _retryHandlerOptionsMonitor = retryHandlerOptionsMonitor;
        }

        #region Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void Configure(LoadBalancerClientOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in LoadBalancerClientOptions>

        #region Implementation of IConfigureNamedOptions<in LoadBalancerClientOptions>

        /// <inheritdoc/>
        public void Configure(string name, LoadBalancerClientOptions options)
        {
            if (options.RetryHandler == null)
            {
                var retryHandlerOptions = _retryHandlerOptionsMonitor.Get(name);
                options.RetryHandler = new HttpClientLoadBalancerErrorHandler(retryHandlerOptions);
            }
        }

        #endregion Implementation of IConfigureNamedOptions<in LoadBalancerClientOptions>
    }
}