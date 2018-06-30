using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using System;

namespace Ribbon.Client.Http.Options
{
    public class HttpClientFactoryOptionsSetup : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        private readonly IOptionsMonitor<LoadBalancerClientOptions> _loadBalancerClientOptionsMonitor;

        internal class HttpClientFactoryConfig
        {
            public HttpClientFactoryConfig()
            {
                Timeout = TimeSpan.FromSeconds(10);
            }

            public TimeSpan Timeout { get; set; }
        }

        private readonly IConfiguration _configuration;

        public HttpClientFactoryOptionsSetup(IServiceProvider services, IOptionsMonitor<LoadBalancerClientOptions> loadBalancerClientOptionsMonitor)
        {
            _loadBalancerClientOptionsMonitor = loadBalancerClientOptionsMonitor;
            _configuration = services.GetService<IConfiguration>();
        }

        #region Implementation of IConfigureOptions<in HttpClientFactoryOptions>

        /// <inheritdoc/>
        public void Configure(HttpClientFactoryOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in HttpClientFactoryOptions>

        #region Implementation of IConfigureNamedOptions<in HttpClientFactoryOptions>

        /// <inheritdoc/>
        public void Configure(string name, HttpClientFactoryOptions options)
        {
            var ribbonSection = _configuration?.GetSection(name)?.GetSection("ribbon");

            var config = ribbonSection == null ? new HttpClientFactoryConfig() : ribbonSection.Get<HttpClientFactoryConfig>();

            options.HttpClientActions.Add(s =>
            {
                s.Timeout = config.Timeout;
            });

            options.HttpMessageHandlerBuilderActions.Add(s =>
            {
                var loadBalancerClientOptions = _loadBalancerClientOptionsMonitor.Get(name);
                s.AdditionalHandlers.Add(new LoadBalancerClientHandler(name, loadBalancerClientOptions));
            });
        }

        #endregion Implementation of IConfigureNamedOptions<in HttpClientFactoryOptions>
    }
}