using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.Client.Http.Options
{
    public class SameRetryHandler : DelegatingHandler
    {
        private readonly string _name;
        private readonly LoadBalancerClientOptions _loadBalancerClientOptions;
        private readonly ILogger<SameRetryHandler> _logger;

        public SameRetryHandler(string name, LoadBalancerClientOptions loadBalancerClientOptions, ILogger<SameRetryHandler> logger)
        {
            _name = name;
            _loadBalancerClientOptions = loadBalancerClientOptions;
            _logger = logger ?? NullLogger<SameRetryHandler>.Instance;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var retryHandler = _loadBalancerClientOptions.RetryHandler;
            var maxRetriesOnSameServer = retryHandler.GetMaxRetriesOnSameServer() + 1;

            if (maxRetriesOnSameServer <= 0)
            {
                maxRetriesOnSameServer = 1;
            }

            for (var i = 0; i < maxRetriesOnSameServer; i++)
            {
                try
                {
                    var responseMessage = await base.SendAsync(request, cancellationToken);

                    if (responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        responseMessage.Dispose();
                        throw new ClientException(ClientException.ErrorType.ServerThrottled);
                    }

                    return responseMessage;
                }
                catch (Exception e)
                {
                    if (i + 1 == maxRetriesOnSameServer)
                    {
                        throw new ClientException(ClientException.ErrorType.NumberofRetriesExceeded,
                            $"Number of retries exceeded max {maxRetriesOnSameServer - 1} retries, while making a call for: {request.RequestUri}",
                            e);
                    }

                    if (!retryHandler.IsRetriableException(e, true))
                    {
                        throw;
                    }

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(e, $"Got error {e} when executed on {request.RequestUri}");
                    }
                }
            }

            //todo:throw exception
            return null;
        }
    }
}