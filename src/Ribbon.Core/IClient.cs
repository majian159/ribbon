using Ribbon.Client.Config;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.Client
{
    public interface IClient
    {
        Task<object> ExecuteAsync(object request, IClientConfig clientConfig, CancellationToken cancellationToken);
    }

    public static class ClientExtensions
    {
        public static async Task<TResponse> ExecuteAsync<TRequest, TResponse>(this IClient client, TRequest request, IClientConfig clientConfig, CancellationToken cancellationToken)
            where TRequest : ClientRequest
            where TResponse : IResponse
        {
            return (TResponse)await client.ExecuteAsync(request, clientConfig, CancellationToken.None);
        }

        public static Task<TResponse> ExecuteAsync<TRequest, TResponse>(this IClient client, TRequest request,
            IClientConfig clientConfig)
            where TRequest : ClientRequest
            where TResponse : IResponse
        {
            return client.ExecuteAsync<TRequest, TResponse>(request, clientConfig, CancellationToken.None);
        }
    }
}