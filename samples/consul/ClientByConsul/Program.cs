using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ribbon.Client;
using Ribbon.Client.Http;
using Ribbon.LoadBalancer.ConsulDiscovery;
using Steeltoe.Discovery.Consul.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientByConsul
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddHttpClient()
                .AddSingleton<IConfiguration>(configuration)
                .AddConsulDiscoveryClient(configuration)
                .AddRibbonClient(b => b.AddHttpClient().AddConsulDiscovery());

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<IHttpClientFactory>().CreateClient("timeService");
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
