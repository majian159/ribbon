using Ribbon.LoadBalancer;
using Ribbon.LoadBalancer.Util;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.Client.Http.Options
{
    public class LoadBalancerClientHandler : DelegatingHandler
    {
        private readonly string _name;
        private readonly LoadBalancerClientOptions _loadBalancerClientOptions;

        public LoadBalancerClientHandler(string name, LoadBalancerClientOptions loadBalancerClientOptions)
        {
            _name = name;
            _loadBalancerClientOptions = loadBalancerClientOptions;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var loadBalancer = _loadBalancerClientOptions.LoadBalancer;

            var retryHandler = _loadBalancerClientOptions.RetryHandler;

            var maxRetriesOnNextServer = retryHandler.GetMaxRetriesOnNextServer() + 1;
            if (maxRetriesOnNextServer <= 0)
            {
                maxRetriesOnNextServer = 1;
            }

            Server server = null;

            for (var i = 0; i < maxRetriesOnNextServer; i++)
            {
                try
                {
                    server = loadBalancer.ChooseServer(null);

                    if (server == null)
                    {
                        throw new ClientException(ClientException.ErrorType.General, "Load balancer does not have available server for client: " + _name);
                    }

                    if (server.Host == null)
                    {
                        throw new ClientException(ClientException.ErrorType.General, "Invalid Server for :" + server);
                    }

                    request.RequestUri = LoadBalancerUtil.ReconstructUriWithServer(server, request.RequestUri);

                    return base.SendAsync(request, cancellationToken);
                }
                catch (Exception e)
                {
                    if (retryHandler.IsCircuitTrippingException(e) && server != null)
                    {
                        loadBalancer.MarkServerDown(server);
                    }

                    if (!retryHandler.IsRetriableException(e, false) || i == maxRetriesOnNextServer)
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