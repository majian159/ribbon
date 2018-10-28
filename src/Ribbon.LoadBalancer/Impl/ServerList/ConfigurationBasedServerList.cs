using System.Linq;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.ServerList
{
    public class ConfigurationBasedServerList : IServerList<Server>
    {
        private readonly LoadBalancerConfig _settings;

        public ConfigurationBasedServerList(LoadBalancerConfig settings)
        {
            _settings = settings;
        }

        #region Implementation of IServerList<out Server>

        /// <inheritdoc/>
        public Task<Server[]> GetInitialListOfServersAsync()
        {
            return GetUpdatedListOfServersAsync();
        }

        /// <inheritdoc/>
        public Task<Server[]> GetUpdatedListOfServersAsync()
        {
            var listOfServers = _settings.ListOfServers;

            Server[] servers;
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