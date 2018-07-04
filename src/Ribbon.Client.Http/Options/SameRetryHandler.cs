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

        public SameRetryHandler(string name, LoadBalancerClientOptions loadBalancerClientOptions)
        {
            _name = name;
            _loadBalancerClientOptions = loadBalancerClientOptions;
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
                    if (!retryHandler.IsRetriableException(e, true) || i == maxRetriesOnSameServer)
                    {
                        throw;
                    }
                    //todo:logging
                    continue;
                }
            }

            //todo:throw exception
            return null;
        }
    }
}