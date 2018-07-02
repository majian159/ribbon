using Ribbon.LoadBalancer.Util;
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

            var server = loadBalancer.ChooseServer(null);

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
    }
}