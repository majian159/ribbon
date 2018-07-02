using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Ribbon.Options
{
    public class RibbonOptionsSetup<TOptions> : IConfigureNamedOptions<TOptions> where TOptions : class
    {
        private readonly IConfiguration _configuration;

        public RibbonOptionsSetup(IServiceProvider services)
        {
            _configuration = services.GetService<IConfiguration>();
        }

        #region Implementation of IConfigureOptions<in TOptions>

        /// <inheritdoc/>
        public void Configure(TOptions options)
        {
            Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        }

        #endregion Implementation of IConfigureOptions<in TOptions>

        #region Implementation of IConfigureNamedOptions<in TOptions>

        /// <inheritdoc/>
        public void Configure(string name, TOptions options)
        {
            if (_configuration == null)
            {
                return;
            }
            var ribbonSection = _configuration.GetSection(name).GetSection("ribbon");

            ribbonSection.Bind(options);
        }

        #endregion Implementation of IConfigureNamedOptions<in TOptions>
    }
}