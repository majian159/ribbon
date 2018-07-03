using System;

namespace Ribbon.LoadBalancer
{
    public interface ILoadBalancerSettings
    {
        string LoadBalancerRuleTypeName { get; }
        string LoadBalancerPingTypeName { get; }
        string LoadBalancerServerListTypeName { get; }
        string ServerListUpdaterTypeName { get; }
        TimeSpan LoadBalancerPingInterval { get; }
    }

    public class LoadBalancerConfig : ILoadBalancerSettings
    {
        public static readonly LoadBalancerConfig Default = new LoadBalancerConfig();

        public LoadBalancerConfig()
        {
            LoadBalancerPingInterval = TimeSpan.FromSeconds(30);
            ServerListRefreshInterval = TimeSpan.FromSeconds(30);
        }

        public string[] ListOfServers { get; set; }
        public TimeSpan ServerListRefreshInterval { get; set; }

        #region Implementation of ILoadBalancerSettings

        /// <inheritdoc/>
        public string LoadBalancerRuleTypeName { get; set; }

        /// <inheritdoc/>
        public string LoadBalancerPingTypeName { get; set; }

        /// <inheritdoc/>
        public string LoadBalancerServerListTypeName { get; set; }

        /// <inheritdoc/>
        public string ServerListUpdaterTypeName { get; set; }

        /// <inheritdoc/>
        public TimeSpan LoadBalancerPingInterval { get; }

        #endregion Implementation of ILoadBalancerSettings
    }
}