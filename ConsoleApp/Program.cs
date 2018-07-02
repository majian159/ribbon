using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ribbon.Client;
using Ribbon.Client.Http;
using Ribbon.LoadBalancer.Consul;
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
                    {"client1:ribbon:MaxAutoRetries","0" },
                    {"client1:ribbon:MaxAutoRetriesNextServer","1" },
                    {"client1:ribbon:OkToRetryOnAllOperations","true" },
                    {"client1:ribbon:ListOfServers:0","https://www.baidu.com" },
                    {"client1:ribbon:ListOfServers:1","http://www.baidu.com" },
                    {"client1:ribbon:Timeout","00:02:00" },
                    {"cs.wechat:ribbon:LoadBalancerServerListTypeName",typeof(ConsulServerList).FullName }
                })
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddSingleton(new ConsulClient(s => s.Address = new Uri("http://192.168.100.150:8500")))
                .AddHttpClient()
                .AddSingleton<IConfiguration>(configuration)
                .AddRibbonClient(b => b.AddHttpClient().AddConsulDiscovery());

            var services = serviceCollection.BuildServiceProvider();

            var clientFactory = services.GetService<IClientFactory>();

            var client2 = clientFactory.CreateClient("client1");
            var client = clientFactory.CreateClient("cs.wechat");

            Thread.Sleep(1000);

            while (true)
            {
                var t = client.ExecuteAsync<HttpRequest, IHttpResponse>(new HttpRequest(new Uri("http://t/accessToken/wx52320fa3039da0ab")), new ExecuteOptions()).GetAwaiter()
                    .GetResult();
                Console.WriteLine(t.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                t = client2.ExecuteAsync<HttpRequest, IHttpResponse>(new HttpRequest(new Uri("http://t/")), new ExecuteOptions()).GetAwaiter()
                    .GetResult();
                Console.WriteLine(t.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Console.ReadLine();
                // robbinHttpClient.

                // Console.WriteLine(server.Id); Console.ReadLine();
            }

            return;
        }
    }
}