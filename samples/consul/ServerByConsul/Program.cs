using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Steeltoe.Discovery.Consul.Client;
using System.Threading.Tasks;

namespace ServerByConsul
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var server1 = CreateWebHostBuilder(1).Build().RunAsync();
            var server2 = CreateWebHostBuilder(2).Build().RunAsync();
            Task.WaitAll(server1, server2);
        }

        public static IWebHostBuilder CreateWebHostBuilder(int serverId)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("server" + serverId + ".json")
                .Build();
            return WebHost.CreateDefaultBuilder()
                .UseConfiguration(configuration)
                .ConfigureAppConfiguration(b => { b.AddJsonFile("server" + serverId + ".json"); })
                .UseStartup<Startup>()
                .UseDiscoveryClient();
        }
    }
}