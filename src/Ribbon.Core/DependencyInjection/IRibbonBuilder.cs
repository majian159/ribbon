using Microsoft.Extensions.DependencyInjection;

namespace Ribbon
{
    public interface IRibbonBuilder
    {
        IServiceCollection Services { get; }
    }
}