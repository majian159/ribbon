using Microsoft.Extensions.DependencyInjection;

namespace Ribbon
{
    public class RibbonBuilder : IRibbonBuilder
    {
        public RibbonBuilder(IServiceCollection services)
        {
            Services = services;
        }

        #region Implementation of IRibbonBuilder

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        #endregion Implementation of IRibbonBuilder
    }
}