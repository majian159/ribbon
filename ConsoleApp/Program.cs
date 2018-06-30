using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ribbon.Client;
using Ribbon.Client.Http;
using Ribbon.LoadBalancer;
using System;
using System.Net.Http;
using System.Threading;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddOptions()
                .Configure<LoadBalancerSettings>("client1", s =>
                {
                    s.ListOfServers = new[] { "http://www.baidu.com" };
                })
                .ConfigureOptions<LoadBalancerClientOptionsSetup>()
                .ConfigureOptions<LoadBalancerOptionsSetup>()
                .Configure<LoadBalancerClientOptions>("client1", s => { })
                .Configure<LoadBalancerClientOptions>("client1", s => { })
                .BuildServiceProvider();

            var com = services.GetService<IOptionsMonitor<LoadBalancerClientOptions>>();
            var co = com.Get("client1");

            var client = new RobbinHttpClient(new HttpClient(), co.LoadBalancer, co.RetryHandler);

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