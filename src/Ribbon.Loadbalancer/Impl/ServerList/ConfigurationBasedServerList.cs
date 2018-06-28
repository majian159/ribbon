using Ribbon.Client.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.ServerList
{
    public class ConfigurationBasedServerList : IServerList<Server>
    {
        private readonly IClientConfig _clientConfig;

        public ConfigurationBasedServerList(IClientConfig clientConfig)
        {
            _clientConfig = clientConfig;
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
            var value = _clientConfig.Get<string>(CommonClientConfigKey.ListOfServers);
            IReadOnlyList<Server> servers = string.IsNullOrEmpty(value) ? Enumerable.Empty<Server>().ToArray() : value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => new Server(id)).ToArray();
            return Task.FromResult(servers);
        }

        #endregion Implementation of IServerList<out Server>
    }
}