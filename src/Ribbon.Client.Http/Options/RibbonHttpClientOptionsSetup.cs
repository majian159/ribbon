using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Ribbon.Client.Http
{
    public class RibbonHttpClientOptionsSetup : IConfigureNamedOptions<RibbonHttpClientOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerClientOptions> _loadBalancerClientOptionsMonitor;
        private readonly IHttpClientFactory _httpClientFactory;

        public RibbonHttpClientOptionsSetup(IOptionsMonitor<LoadBalancerClientOptions> loadBalancerClientOptionsMonitor, IHttpClientFactory httpClientFactory)
        {
            _loadBalancerClientOptionsMonitor = loadBalancerClientOptionsMonitor;
            _httpClientFactory = httpClientFactory;
        }

        #region Implementation of IConfigureOptions<in RobbinHttpClientOptions>

        /// <inheritdoc/>
        public void Configure(RibbonHttpClientOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in RobbinHttpClientOptions>

        #region Implementation of IConfigureNamedOptions<in RobbinHttpClientOptions>

        /// <inheritdoc/>
        public void Configure(string name, RibbonHttpClientOptions options)
        {
            var loadBalancerClientOptions = _loadBalancerClientOptionsMonitor.Get(name);

            options.LoadBalancer = loadBalancerClientOptions.LoadBalancer;
            options.HttpClient = _httpClientFactory.CreateClient(name);
            options.RetryHandler = loadBalancerClientOptions.RetryHandler;
        }

        #endregion Implementation of IConfigureNamedOptions<in RobbinHttpClientOptions>
    }
}