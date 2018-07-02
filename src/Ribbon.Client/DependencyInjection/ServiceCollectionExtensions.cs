using Microsoft.Extensions.DependencyInjection;
using Ribbon.Client.Impl;
using Ribbon.Client.Options;
using Ribbon.LoadBalancer;
using System;

namespace Ribbon.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IRibbonBuilder AddRibbonClient(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .ConfigureOptions<RibbonOptionsSetup<LoadBalancerConfig>>()
                .ConfigureOptions<LoadBalancerOptionsSetup>()
                .ConfigureOptions<RibbonOptionsSetup<LoadBalancerClientConfig>>()
                .ConfigureOptions<RibbonOptionsSetup<RetryHandlerConfig>>()
                .ConfigureOptions<LoadBalancerClientOptionsSetup>()
                .AddSingleton<IClientFactory, DefaultClientFactory>();

            return new RibbonBuilder(services);
        }

        public static IServiceCollection AddRibbonClient(this IServiceCollection services, Action<IRibbonBuilder> build)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (build == null)
            {
                throw new ArgumentNullException(nameof(build));
            }

            var builder = services.AddRibbonClient();

            build(builder);

            return builder.Services;
        }
    }
}