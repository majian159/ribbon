using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.ServerList
{
    public class ConfigurationBasedServerList : IServerList<Server>
    {
        private readonly LoadBalancerSettings _settings;

        public ConfigurationBasedServerList(LoadBalancerSettings settings)
        {
            _settings = settings;
        }

        #region Implementation of IServerList<out Server>

        /// <inheritdoc/>
        public Task<IReadOnlyList<Server>> GetInitialListOfServersAsync()
        {
            return GetUpdatedListOfServersAsync();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<Server>> GetUpdatedListOfServersAsync()
        {
            var listOfServers = _settings.ListOfServers;

            IReadOnlyList<Server> servers;
            if (listOfServers == null || !listOfServers.Any())
            {
                servers = Enumerable.Empty<Server>().ToArray();
                return Task.FromResult(servers);
            }

            servers = listOfServers.Where(id => !string.IsNullOrEmpty(id)).Select(id => new Server(id)).ToArray();
            return Task.FromResult(servers);
        }

        #endregion Implementation of IServerList<out Server>
    }
}