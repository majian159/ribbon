namespace Ribbon.LoadBalancer
{
    public class LoadBalancerOptions
    {
        public IRule Rule { get; set; }
        public IPing Ping { get; set; }
        public IServerList<Server> ServerList { get; set; }
        public IServerListUpdater ServerListUpdater { get; set; }
        public ILoadBalancerSettings Settings { get; set; }
    }
}