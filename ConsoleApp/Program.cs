using Ribbon.Client;
using Ribbon.Client.Config;
using Ribbon.Client.Http;
using System;
using System.Threading;

namespace ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var clientFactory = new DefaultClientFactory();

            var clientConfig = new DefaultClientConfig();
            clientConfig.Properties[CommonClientConfigKey.ListOfServers] =
                "http://www.baidu.com,https://www.baidu.com";

            // clientFactory.RegisterNamedLoadBalancerFromclientConfig("test", clientConfig);

            var client = clientFactory.CreateNamedClient("test", clientConfig);

            Thread.Sleep(1000);

            while (true)
            {
                var t = client.ExecuteAsync<HttpRequest, IHttpResponse>(new HttpRequest(new Uri("http://t")), clientConfig).GetAwaiter()
                    .GetResult();
                Console.ReadLine();
                // robbinHttpClient.

                // Console.WriteLine(server.Id); Console.ReadLine();
            }
        }
    }
}