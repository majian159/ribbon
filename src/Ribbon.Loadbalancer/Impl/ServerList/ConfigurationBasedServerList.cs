using Ribbon.Client.Config;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IReadOnlyList<Server> GetInitialListOfServers()
        {
            return GetUpdatedListOfServers();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Server> GetUpdatedListOfServers()
        {
            var value = _clientConfig.Get<string>(CommonClientConfigKey.ListOfServers);
            return string.IsNullOrEmpty(value) ? Enumerable.Empty<Server>().ToArray() : value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => new Server(id)).ToArray();
        }

        #endregion Implementation of IServerList<out Server>
    }
}