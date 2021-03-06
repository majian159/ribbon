﻿using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl
{
    public class DynamicServerListLoadBalancer : BaseLoadBalancer
    {
        private readonly IServerList<Server> _serverList;
        private readonly IServerListUpdater _serverListUpdater;
        private readonly IServerListFilter<Server> _serverListFilter;

        public DynamicServerListLoadBalancer(string name, LoadBalancerOptions options)
            : this(name, options.Rule, options.Ping, options.ServerList, options.ServerListUpdater, options.ServerListFilter, options.Settings)
        {
        }

        /// <inheritdoc/>
        public DynamicServerListLoadBalancer(string name, IRule rule, IPing ping, IServerList<Server> serverList, IServerListUpdater serverListUpdater, IServerListFilter<Server> serverListFilter, ILoadBalancerSettings settings, ILogger logger = null) : base(name, rule, ping, settings, logger)
        {
            _serverList = serverList;
            _serverListUpdater = serverListUpdater;
            _serverListFilter = serverListFilter;

            UpdateListOfServersAsync().GetAwaiter().GetResult();
            if (serverListUpdater != null)
            {
                var updateProcessing = false;
                serverListUpdater.Start(async () =>
                {
                    if (updateProcessing)
                    {
                        return;
                    }

                    updateProcessing = true;

                    try
                    {
                        await UpdateListOfServersAsync();
                    }
                    finally
                    {
                        updateProcessing = false;
                    }
                });
            }
        }

        private async Task UpdateListOfServersAsync()
        {
            var servers = await _serverList.GetUpdatedListOfServersAsync();

            if (_serverListFilter != null)
            {
                servers = _serverListFilter.GetFilteredListOfServers(servers);
            }

            UpdateAllServerList(servers);
        }

        private void UpdateAllServerList(Server[] servers)
        {
            foreach (var server in servers)
            {
                server.IsAlive = true;
            }
            SetServersList(servers);
            ForceQuickPing();
        }
    }
}