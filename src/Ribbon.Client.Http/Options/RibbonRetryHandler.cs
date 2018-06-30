/*using Ribbon.LoadBalancer;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.Client.Http.Options
{
    public class RibbonRetryHandler : DelegatingHandler
    {
        private readonly LoadBalancerClientOptions _options;

        public RibbonRetryHandler(LoadBalancerClientOptions options)
        {
            _options = options;
        }

        private async Task<HttpResponseMessage> SameServerSendAsync(Server server, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryHandler = _options.RetryHandler;
            var retryCount = retryHandler.GetMaxRetriesOnSameServer() + 1;
            for (var i = 0; i < retryCount; i++)
            {
                try
                {
                    return await base.SendAsync(request, cancellationToken);
                }
                catch (Exception e) when (i == retryCount - 1)
                {
                    throw new ClientException(ClientException.ErrorType.NumberofRetriesExceeded, $"Number of retries exceeded max {retryHandler.GetMaxRetriesOnSameServer()} retries, while making a call for: {server}", e);
                }
                catch (Exception e) when (!retryHandler.IsRetriableException(e, true))
                {
                    throw;
                }
                catch (Exception e)
                {
                    //log
                }
            }

            throw null;
        }

        ClientException CreateClientException(Exception exception)
        {
            return null;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryHandler = _options.RetryHandler;
            var retryCount = retryHandler.GetMaxRetriesOnNextServer() + 1;
            var loadBalancer = _options.LoadBalancer;
            for (var i = 0; i < retryCount; i++)
            {
                var server = loadBalancer.ChooseServer(null);
                request.RequestUri = LoadBalancerContext.ReconstructUriWithServer(server, request.RequestUri);
                try
                {
                    return await SameServerSendAsync(server, request, cancellationToken);
                }
                catch (Exception e)
                {
                    if (i == retryCount - 1)
                    {
                        if (retryHandler.IsCircuitTrippingException(e))
                        {
                            loadBalancer.MarkServerDown(server);
                        }
                        throw new ClientException(ClientException.ErrorType.NumberofRetriesNextserverExceeded, $"Number of retries on next server exceeded max {retryHandler.GetMaxRetriesOnSameServer()} retries, while making a call for: {server}", e);
                    }

                    if (!retryHandler.IsRetriableException(e, true))
                    {
                        if (retryHandler.IsCircuitTrippingException(e))
                        {
                            loadBalancer.MarkServerDown(server);
                        }
                        throw;
                    }
                    //log
                }
            }

            throw null;
        }
    }
}*/