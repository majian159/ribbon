using Ribbon.Client.Config;
using Ribbon.Client.Http;
using Ribbon.LoadBalancer;
using Ribbon.LoadBalancer.Impl;
using Ribbon.LoadBalancer.Impl.Ping;
using Ribbon.LoadBalancer.Impl.Rule;
using Ribbon.LoadBalancer.Impl.ServerList;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Ribbon.Client
{
    public class DefaultClientFactory : IClientFactory
    {
        private readonly ConcurrentDictionary<string, IClient> _clients = new ConcurrentDictionary<string, IClient>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ILoadBalancer> _loadBalancers = new ConcurrentDictionary<string, ILoadBalancer>(StringComparer.OrdinalIgnoreCase);

        #region Implementation of IClientFactory

        /// <inheritdoc/>
        public IClient GetNamedClient(string name)
        {
            return _clients.TryGetValue(name, out var client) ? client : null;
        }

        /// <inheritdoc/>
        public IClient CreateNamedClient(string name, IClientConfig clientConfig)
        {
            var loadBalancer = GetNamedLoadBalancer(name);
            if (loadBalancer == null)
            {
                loadBalancer = RegisterNamedLoadBalancerFromclientConfig(name, clientConfig);
                if (loadBalancer == null)
                {
                    _clients.TryRemove(name, out _);
                    return null;
                }
            }

            var client = new RobbinHttpClient(new HttpClient(), loadBalancer, clientConfig);

            return _clients.AddOrUpdate(name, client, (key, old) => client);
        }

        /// <inheritdoc/>
        public ILoadBalancer GetNamedLoadBalancer(string name)
        {
            return _loadBalancers.TryGetValue(name, out var loadBalancer) ? loadBalancer : null;
        }

        /// <inheritdoc/>
        public ILoadBalancer RegisterNamedLoadBalancerFromclientConfig(string name, IClientConfig clientConfig)
        {
            /*            var ruleTypeName = clientConfig.Get(CommonClientConfigKey.LoadBalancerRuleClassName,
                            typeof(RoundRobinRule).FullName);
                        var pingTypeName = clientConfig.Get(CommonClientConfigKey.LoadBalancerPingClassName,
                            typeof(NoOpPing).FullName);*/

            var rule = new RoundRobinRule();
            var ping = new NoOpPing();
            IServerList<Server> serverList = new ConfigurationBasedServerList(clientConfig);
            var loadBalancer = new DefaultLoadBalancer(rule, ping, serverList, clientConfig);
            rule.LoadBalancer = loadBalancer;

            return _loadBalancers.AddOrUpdate(name, k => loadBalancer, (key, old) => loadBalancer);
        }

        #endregion Implementation of IClientFactory
    }
}