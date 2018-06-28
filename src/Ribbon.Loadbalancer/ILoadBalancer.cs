using System.Collections.Generic;

namespace Ribbon.LoadBalancer
{
    public interface ILoadBalancer
    {
        void AddServers(List<Server> newServers);

        Server ChooseServer(object key);

        void MarkServerDown(Server server);

        IReadOnlyList<Server> GetReachableServers();

        IReadOnlyList<Server> GetAllServers();
    }
}