using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl
{
    public class DefaultLoadBalancer : ILoadBalancer
    {
        private readonly IRule _rule;
        private readonly IPing _ping;
        private readonly LoadBalancerSettings _settings;
        private readonly ConcurrentBag<Server> _servers = new ConcurrentBag<Server>();
        private readonly Timer _pingTimer;
        protected TimeSpan PingInterval { get; }
        public IServerList<Server> ServerList { get; set; }

        // protected int MaxTotalPingTime { get; } = 5;

        public DefaultLoadBalancer(IRule rule, IPing ping, IServerList<Server> serverList, LoadBalancerSettings settings)
        {
            _rule = rule;
            _ping = ping;
            _settings = settings;
            ServerList = serverList;

            var updatedListOfServers = serverList?.GetUpdatedListOfServersAsync().GetAwaiter().GetResult();
            while (_servers.TryPeek(out _))
            {
            }

            if (updatedListOfServers != null)
            {
                foreach (var server in updatedListOfServers)
                {
                    _servers.Add(server);
                }
            }

            _pingTimer = new Timer(s =>
                {
                    Parallel.ForEach(_servers.ToArray(), async server =>
                    {
                        server.IsAlive = await ping.IsAliveAsync(server);
                    });
                }, null, TimeSpan.Zero, PingInterval);
            PingInterval = _settings.LoadBalancerPingInterval;
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
    }
}