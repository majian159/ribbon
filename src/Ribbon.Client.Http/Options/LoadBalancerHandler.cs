using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private readonly ILogger<LoadBalancerClientHandler> _logger;

        public LoadBalancerClientHandler(string name, LoadBalancerClientOptions loadBalancerClientOptions, ILogger<LoadBalancerClientHandler> logger)
        {
            _name = name;
            _loadBalancerClientOptions = loadBalancerClientOptions;
            _logger = logger ?? NullLogger<LoadBalancerClientHandler>.Instance;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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

                    return await base.SendAsync(request, cancellationToken);
                }
                catch (Exception e)
                {
                    var isNumberofRetriesNextServerExceeded = i + 1 == maxRetriesOnNextServer;

                    if (isNumberofRetriesNextServerExceeded)
                    {
                        e = new ClientException(ClientException.ErrorType.NumberofRetriesNextServerExceeded,
                            $"Number of retries on next server exceeded max {maxRetriesOnNextServer - 1} retries, while making a call for: {server}",
                            e);
                    }

                    if (retryHandler.IsCircuitTrippingException(e) && server != null)
                    {
                        loadBalancer.MarkServerDown(server);
                    }

                    if (isNumberofRetriesNextServerExceeded)
                    {
                        throw (ClientException)e;
                    }

                    if (!retryHandler.IsRetriableException(e, false) || i + 1 == maxRetriesOnNextServer)
                    {
                        throw;
                    }

                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug(e, $"Got error {e} when executed on server {server}");
                    }
                }
            }

            //todo:throw exception
            return null;
        }
    }
}