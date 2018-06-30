using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ribbon.Client;
using Ribbon.Client.Http;
using Ribbon.Client.Http.Options;
using Ribbon.Client.Options;
using Ribbon.LoadBalancer;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
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
                    {"client1:ribbon:MaxAutoRetries","0" },
                    {"client1:ribbon:MaxAutoRetriesNextServer","1" },
                    {"client1:ribbon:OkToRetryOnAllOperations","true" },
                    {"client1:ribbon:ListOfServers:0","https://www.baidu.com" },
                    {"client1:ribbon:ListOfServers:1","http://www.baidu.com" },
                    {"client1:ribbon:Timeout","00:02:00" }
                })
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddHttpClient()
                .AddSingleton<IConfiguration>(configuration)
                .ConfigureOptions<HttpClientFactoryOptionsSetup>()
                .ConfigureOptions<RibbonOptionsSetup<RetryHandlerConfig>>()
                .ConfigureOptions<RibbonOptionsSetup<LoadBalancerConfig>>()
                .ConfigureOptions<LoadBalancerClientOptionsSetup>()
                .ConfigureOptions<LoadBalancerOptionsSetup>()
                .ConfigureOptions<RibbonHttpClientOptionsSetup>();

            var services = serviceCollection.BuildServiceProvider();



            var rcom = services.GetService<IOptionsMonitor<RibbonHttpClientOptions>>();
            var rco = rcom.Get("client1");

            

            var client = new RibbonHttpClient(rco.HttpClient);

            Thread.Sleep(1000);

            while (true)
            {
                var t = client.ExecuteAsync<HttpRequest, IHttpResponse>(new HttpRequest(new Uri("http://t")), new ExecuteOptions()).GetAwaiter()
                    .GetResult();
                Console.WriteLine(t.StatusLine+t.RequestedUri);
                Console.ReadLine();
                // robbinHttpClient.

                // Console.WriteLine(server.Id); Console.ReadLine();
            }

            return;
        }
    }
}