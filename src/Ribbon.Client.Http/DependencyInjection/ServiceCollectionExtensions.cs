using Microsoft.Extensions.DependencyInjection;
using Ribbon.Client.Http.Options;

namespace Ribbon.Client.Http
{
    public static class ServiceCollectionExtensions
    {
        public static IRibbonBuilder AddHttpClient(this IRibbonBuilder builder)
        {
            builder
                .Services
                .ConfigureOptions<HttpClientFactoryOptionsSetup>()
                .ConfigureOptions<RibbonHttpClientOptionsSetup>();

            return builder;
        }
    }
}