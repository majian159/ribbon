using Consul;
using Microsoft.Extensions.Options;
using Ribbon.LoadBalancer;
using Steeltoe.Discovery.Consul.Discovery;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribbon.Consul
{
    public class ConsulServiceList : IServerList<Server>
    {
        private readonly string _clientName;
        private readonly ConsulClient _consulClient;
        private readonly ConsulDiscoveryOptions _consulDiscoveryOptions;

        public ConsulServiceList(string clientName, ConsulClient consulClient, IOptionsMonitor<ConsulDiscoveryOptions> consulDiscoveryOptionsMonitor)
        {
            _clientName = clientName;
            _consulClient = consulClient;
            _consulDiscoveryOptions = consulDiscoveryOptionsMonitor.Get(clientName);
        }

        #region Implementation of IServerList<ConsulServer>

        /// <inheritdoc/>
        public Task<IReadOnlyList<Server>> GetInitialListOfServersAsync()
        {
            return GetServersAsync();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<Server>> GetUpdatedListOfServersAsync()
        {
            return GetServersAsync();
        }

        #endregion Implementation of IServerList<ConsulServer>

        private async Task<IReadOnlyList<Server>> GetServersAsync()
        {
            var response = await _consulClient.Health.Service(_clientName, _consulDiscoveryOptions.DefaultQueryTag, _consulDiscoveryOptions.QueryPassing);
            return response.Response.Select(s => new ConsulServer(s)).ToArray();
        }
    }
}