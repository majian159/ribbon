using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
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
                    return responseMessage;
                }
                catch (Exception e)
                {
                    if (!retryHandler.IsRetriableException(e, true) || i + 1 == maxRetriesOnSameServer)
                    {
                        throw;
                    }
                    _logger.LogError(e, "");
                }
            }

            //todo:throw exception
            return null;
        }
    }
}