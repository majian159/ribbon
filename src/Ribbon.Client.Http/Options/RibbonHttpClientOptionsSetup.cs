using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Ribbon.Client.Http
{
    public class RibbonHttpClientOptionsSetup : IConfigureNamedOptions<RibbonHttpClientOptions>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<LoadBalancerClientOptions> _loadBalancerClientOptionsMonitor;

        public RibbonHttpClientOptionsSetup(IHttpClientFactory httpClientFactory, IOptionsMonitor<LoadBalancerClientOptions> loadBalancerClientOptionsMonitor)
        {
            _httpClientFactory = httpClientFactory;
            _loadBalancerClientOptionsMonitor = loadBalancerClientOptionsMonitor;
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
            options.HttpClient = _httpClientFactory.CreateClient(name);
            options.LoadBalancerClientOptions = _loadBalancerClientOptionsMonitor.Get(name);
        }

        #endregion Implementation of IConfigureNamedOptions<in RobbinHttpClientOptions>
    }
}