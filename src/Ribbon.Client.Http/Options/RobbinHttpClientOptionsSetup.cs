using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Ribbon.Client.Http
{
    public class RobbinHttpClientOptionsSetup : IConfigureNamedOptions<RobbinHttpClientOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerClientOptions> _loadBalancerClientOptionsMonitor;

        public RobbinHttpClientOptionsSetup(IOptionsMonitor<LoadBalancerClientOptions> loadBalancerClientOptionsMonitor)
        {
            _loadBalancerClientOptionsMonitor = loadBalancerClientOptionsMonitor;
        }

        #region Implementation of IConfigureOptions<in RobbinHttpClientOptions>

        /// <inheritdoc/>
        public void Configure(RobbinHttpClientOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in RobbinHttpClientOptions>

        #region Implementation of IConfigureNamedOptions<in RobbinHttpClientOptions>

        /// <inheritdoc/>
        public void Configure(string name, RobbinHttpClientOptions options)
        {
            var loadBalancerClientOptions = _loadBalancerClientOptionsMonitor.Get(name);

            options.LoadBalancer = loadBalancerClientOptions.LoadBalancer;
            options.HttpClient = new HttpClient();
            options.RetryHandler = loadBalancerClientOptions.RetryHandler;
        }

        #endregion Implementation of IConfigureNamedOptions<in RobbinHttpClientOptions>
    }
}