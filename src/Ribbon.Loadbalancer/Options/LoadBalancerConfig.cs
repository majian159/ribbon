using System;

namespace Ribbon.LoadBalancer
{
    public interface ILoadBalancerSettings
    {
        TimeSpan LoadBalancerPingInterval { get; }
    }

    public class LoadBalancerConfig : ILoadBalancerSettings
    {
        public LoadBalancerConfig()
        {
            LoadBalancerPingInterval = TimeSpan.FromSeconds(30);
        }

        public string RuleName { get; set; }
        public string PingName { get; set; }
        public string ServerListName { get; set; }

        public string[] ListOfServers { get; set; }

        #region Implementation of ILoadBalancerSettings

        /// <inheritdoc/>
        public TimeSpan LoadBalancerPingInterval { get; }

        #endregion Implementation of ILoadBalancerSettings
    }
}