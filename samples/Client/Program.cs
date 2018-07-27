using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Feign;
using Ribbon.Client;
using Ribbon.Client.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    [FeignClient(Name = "time", FallbackType = typeof(TimeServiceFallback))]
    public interface ITimeService
    {
        [GoGet("/time")]
        Task<DateTime> GetNowAsync();
    }

    public class TimeServiceFallback : ITimeService
    {
        #region Implementation of ITimeService

        /// <inheritdoc/>
        public Task<DateTime> GetNowAsync()
        {
            return Task.FromResult(DateTime.MinValue);
        }

        #endregion Implementation of ITimeService
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
// {"time:ribbon:MaxAutoRetries","0" }, {"time:ribbon:MaxAutoRetriesNextServer","1" },
// {"time:ribbon:OkToRetryOnAllOperations","true" },
                    {"time:ribbon:ListOfServers:0","http://localhost:6001" },
                    {"time:ribbon:ListOfServers:1","http://localhost:6002" },
// {"time:ribbon:LoadBalancerRuleTypeName",typeof(RoundRobinRule).FullName },
                    {"time:ribbon:Timeout","00:00:05" }
                })
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddHttpClient()
                .AddSingleton<IConfiguration>(configuration)
                .AddRibbonClient(b => b.AddHttpClient());

            // use Feign
            {
                var feignBuilder = new FeignBuilder(serviceCollection.AddFeign().BuildServiceProvider());

                serviceCollection.AddSingleton(feignBuilder.TargetByAttribute<ITimeService>());
                var services = serviceCollection.BuildServiceProvider();

                var timeService = services.GetService<ITimeService>();

                while (true)
                {
                    Task.Run(async () =>
                    {
                        var now = await timeService.GetNowAsync();
                        Console.WriteLine("Content: " + now);
                        Console.ReadLine();
                    }).Wait();
                }
            }

            // use HttpClient
            {
                var services = serviceCollection.BuildServiceProvider();

                var client = services.GetRequiredService<IHttpClientFactory>().CreateClient("time");

                Task.Run(async () =>
                {
                    while (true)
                    {
                        var responseMessage = await client.GetAsync("/time");
                        var uri = responseMessage.RequestMessage.RequestUri;
                        Console.WriteLine($"From Server: {uri.Host}:{uri.Port}");
                        Console.WriteLine("Content: " + await responseMessage.Content.ReadAsStringAsync());
                        Console.ReadLine();
                    }
                }).Wait();
            }
        }
    }
}