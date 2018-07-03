using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Feign;
using Ribbon.Client;
using Ribbon.Client.Http;
using Ribbon.LoadBalancer.ConsulDiscovery;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public interface IBookAggregationCommentGoClient
    {
        [GoGet("/book/{bookId}/commentary/aggregation?userId={userId}&offset={offset}&limit={limit}")]
        Task<AggregationCommentResult> GetAggregationCommentsAsync(long bookId, long userId, int offset, int limit);
    }

    class BookAggregationCommentGoClient: IBookAggregationCommentGoClient
    {
        #region Implementation of IBookAggregationCommentGoClient

        /// <inheritdoc />
        public Task<AggregationCommentResult> GetAggregationCommentsAsync(long bookId, long userId, int offset, int limit)
        {
            return Task.FromResult(new AggregationCommentResult
            {
                Comments = new AggregationCommentModel[0]
            });
        }

        #endregion
    }
    public class AggregationCommentResult
    {
        public AggregationCommentModel[] Comments { get; set; }
        public int TotalCount { get; set; }
    }

    public class AggregationCommentModel
    {
        public long Id { get; set; }
        public int UserSource { get; set; }
        public long SenderId { get; set; }

        private string _senderName;

        public string SenderName
        {
            get => _senderName;
            set => _senderName = string.IsNullOrWhiteSpace(value) ? " " : value;
        }

        public string Title { get; set; }
        public string Content { get; set; }
        public int Score { get; set; }
        public DateTime SendTime { get; set; }
        public int HeadId { get; set; }
        public long ChapterId { get; set; }
        public int ReplyCount { get; set; }
        public int SupportCount { get; set; }
        public bool IsParagraph { get; set; }
        public int Index { get; set; }
        public bool IsSecret { get; set; }
        public bool IsTop { get; set; }
        public int CommentSource { get; set; }
        public int ExCommentId { get; set; }
        public string Classification { get; set; }
        public int Classify { get; set; }
    }
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
                    {"hystrix:command:GetAggregationCommentsAsync:circuitBreaker:forceOpen","true" }
                })
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddSingleton(new ConsulClient(s => s.Address = new Uri("http://192.168.100.150:8500")))
                .AddHttpClient()
                .AddSingleton<IConfiguration>(configuration)
                .AddRibbonClient(b => b.AddHttpClient().AddConsulDiscovery())
                .AddFeign();

            var services = serviceCollection.BuildServiceProvider();

            var goBuilder = services.GetRequiredService<FeignBuilder>();

            var go=goBuilder
                .ClientName("cs.comment")
                .FallbackType(typeof(BookAggregationCommentGoClient))
                .Target<IBookAggregationCommentGoClient>();

            var tt=go.GetAggregationCommentsAsync(2048, 0, 0, 10).GetAwaiter().GetResult();

            return;

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