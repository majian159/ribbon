using Ribbon.Client.Config;
using Ribbon.LoadBalancer;

namespace Ribbon.Client
{
    public interface IClientFactory
    {
        IClient GetNamedClient(string name);

        IClient CreateNamedClient(string name, IClientConfig clientConfig);

        ILoadBalancer GetNamedLoadBalancer(string name);

        ILoadBalancer RegisterNamedLoadBalancerFromclientConfig(string name, IClientConfig clientConfig);
    }
}