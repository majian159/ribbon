using System;

namespace Ribbon.LoadBalancer
{
    public class LoadBalancerSettings
    {
        public LoadBalancerSettings()
        {
            LoadBalancerPingInterval = TimeSpan.FromSeconds(30);
        }

        public string RuleName { get; set; }
        public string PingName { get; set; }
        public string ServerListName { get; set; }

        public TimeSpan LoadBalancerPingInterval { get; set; }
        public string[] ListOfServers { get; set; }
    }
}