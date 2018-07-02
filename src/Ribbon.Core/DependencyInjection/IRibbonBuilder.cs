using Microsoft.Extensions.DependencyInjection;

namespace Ribbon.Client
{
    public interface IRibbonBuilder
    {
        IServiceCollection Services { get; }
    }
}