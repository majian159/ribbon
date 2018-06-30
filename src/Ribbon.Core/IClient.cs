using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.Client
{
    public interface IClient
    {
        Task<object> ExecuteAsync(object request, ExecuteOptions settings, CancellationToken cancellationToken);
    }

    public static class ClientExtensions
    {
        public static async Task<TResponse> ExecuteAsync<TRequest, TResponse>(this IClient client, TRequest request, ExecuteOptions settings, CancellationToken cancellationToken)
            where TRequest : ClientRequest
            where TResponse : IResponse
        {
            return (TResponse)await client.ExecuteAsync(request, settings, CancellationToken.None);
        }

        public static Task<TResponse> ExecuteAsync<TRequest, TResponse>(this IClient client, TRequest request,
            ExecuteOptions settings)
            where TRequest : ClientRequest
            where TResponse : IResponse
        {
            return client.ExecuteAsync<TRequest, TResponse>(request, settings, CancellationToken.None);
        }
    }
}