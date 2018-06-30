using System;

namespace Ribbon.LoadBalancer
{
    public interface ILoadBalancerSettings
    {
        string LoadBalancerRuleTypeName { get; }
        string LoadBalancerPingTypeName { get; }
        string LoadBalancerServerListTypeName { get; }
        TimeSpan LoadBalancerPingInterval { get; }
    }

    public class LoadBalancerConfig : ILoadBalancerSettings
    {
        public LoadBalancerConfig()
        {
            LoadBalancerPingInterval = TimeSpan.FromSeconds(30);
        }

        public string[] ListOfServers { get; set; }

        #region Implementation of ILoadBalancerSettings

        /// <inheritdoc/>
        public string LoadBalancerRuleTypeName { get; set; }

        /// <inheritdoc/>
        public string LoadBalancerPingTypeName { get; set; }

        /// <inheritdoc/>
        public string LoadBalancerServerListTypeName { get; set; }

        /// <inheritdoc/>
        public TimeSpan LoadBalancerPingInterval { get; }

        #endregion Implementation of ILoadBalancerSettings
    }
}