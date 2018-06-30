using Ribbon.LoadBalancer;

namespace Ribbon.Client
{
    public class LoadBalancerOptions
    {
        public IRule Rule { get; set; }
        public IPing Ping { get; set; }
        public IServerList<Server> ServerList { get; set; }
    }
}