using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ribbon.Client;
using Ribbon.Client.Http;
using Ribbon.Client.Options;
using Ribbon.LoadBalancer;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"client1:ribbon:MaxAutoRetries","2" },
                    {"client1:ribbon:MaxAutoRetriesNextServer","3" },
                    {"client1:ribbon:OkToRetryOnAllOperations","true" },
                    {"client1:ribbon:ListOfServers:0","http://www.baidu.com" },
                    {"client1:ribbon:ListOfServers:1","https://www.baidu.com" }
                })
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddSingleton<IConfiguration>(configuration)
                .ConfigureOptions<RibbonOptionsSetup<RetryHandlerConfig>>()
                .ConfigureOptions<RibbonOptionsSetup<LoadBalancerConfig>>()
                .ConfigureOptions<LoadBalancerClientOptionsSetup>()
                .ConfigureOptions<LoadBalancerOptionsSetup>()
                .ConfigureOptions<RobbinHttpClientOptionsSetup>();

            var services = serviceCollection.BuildServiceProvider();

            var rcom = services.GetService<IOptionsMonitor<RobbinHttpClientOptions>>();
            var rco = rcom.Get("client1");

            var client = new RibbonHttpClient(rco);

            Thread.Sleep(1000);

            while (true)
            {
                var t = client.ExecuteAsync<HttpRequest, IHttpResponse>(new HttpRequest(new Uri("http://t")), new ExecuteOptions()).GetAwaiter()
                    .GetResult();
                Console.ReadLine();
                // robbinHttpClient.

                // Console.WriteLine(server.Id); Console.ReadLine();
            }

            return;
        }
    }
}