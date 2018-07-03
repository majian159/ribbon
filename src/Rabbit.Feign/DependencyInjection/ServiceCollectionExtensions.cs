using Microsoft.Extensions.DependencyInjection;
using Rabbit.Feign.Reflective;
using Rabbit.Go.Core.Builder;
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
                .AddTransient<GoBuilder>()
                .AddSingleton<IParameterExpanderLocator, ParameterExpanderLocator>();

            return services;
        }
    }
}