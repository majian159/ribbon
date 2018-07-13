using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ribbon.Client;
using Ribbon.Client.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
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