using Ribbon.LoadBalancer.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl
{
    public class DefaultLoadBalancer : ILoadBalancer, IDisposable
    {
        private readonly IRule _rule;
        private readonly IServerListUpdater _serverListUpdater;
        private readonly ConcurrentBag<Server> _servers = new ConcurrentBag<Server>();
        private readonly Timer _pingTimer;
        protected TimeSpan PingInterval { get; }
        public IServerList<Server> ServerList { get; set; }

        // protected int MaxTotalPingTime { get; } = 5;
        public DefaultLoadBalancer(LoadBalancerOptions options)
            : this(options.Rule, options.Ping, options.ServerList, options.ServerListUpdater, options.Settings)
        {
        }

        internal DefaultLoadBalancer(IRule rule, IPing ping, IServerList<Server> serverList, IServerListUpdater serverListUpdater, ILoadBalancerSettings settings)
        {
            rule.LoadBalancer = this;
            _rule = rule;
            _serverListUpdater = serverListUpdater;
            ServerList = serverList;

            UpdateListOfServersAsync().GetAwaiter().GetResult();
            serverListUpdater?.Start(UpdateListOfServersAsync);

            _pingTimer = new Timer(s =>
                {
                    Parallel.ForEach(_servers.ToArray(), async server =>
                    {
                        server.IsAlive = await ping.IsAliveAsync(server);
                    });
                }, null, TimeSpan.Zero, PingInterval);
            PingInterval = settings.LoadBalancerPingInterval;
        }

        #region Implementation of ILoadBalancer

        /// <inheritdoc/>
        public void AddServers(List<Server> newServers)
        {
            foreach (var server in newServers)
            {
                if (_servers.Contains(server))
                {
                    continue;
                }
                _servers.Add(server);
            }
        }

        /// <inheritdoc/>
        public Server ChooseServer(object key)
        {
            return _rule?.Choose(key);
        }

        /// <inheritdoc/>
        public void MarkServerDown(Server server)
        {
            if (server == null || !server.IsAlive)
            {
                return;
            }

            server.IsAlive = false;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Server> GetReachableServers()
        {
            return _servers.Where(i => i.IsAlive).ToArray();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Server> GetAllServers()
        {
            return _servers.ToArray();
        }

        #endregion Implementation of ILoadBalancer

        private async Task UpdateListOfServersAsync()
        {
            while (_servers.TryTake(out _))
            {
            }
            var servers = await ServerList.GetUpdatedListOfServersAsync();
            foreach (var server in servers)
            {
                server.IsAlive = true;
                _servers.Add(server);
            }
        }

        #region IDisposable

        /// <inheritdoc/>
        public void Dispose()
        {
            _pingTimer?.Dispose();
            _serverListUpdater.Stop();
        }

        #endregion IDisposable
    }
}