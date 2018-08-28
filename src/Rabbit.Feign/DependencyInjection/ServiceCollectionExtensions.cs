using Microsoft.Extensions.DependencyInjection;
using Rabbit.Feign.Codec;
using Rabbit.Feign.Reflective;
using System;

namespace Rabbit.Feign
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFeign(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddTransient<FeignBuilder>()
                .AddSingleton<IParameterExpanderLocator, ParameterExpanderLocator>()
                .AddSingleton<IEncoder, DefaultEncoder>()
                .AddSingleton<IDecoder, DefaultDecoder>();

            return services;
        }
    }
}