using System.Collections.Generic;

namespace Ribbon.LoadBalancer
{
    public interface IServerList<out T> where T : Server
    {
        IReadOnlyList<T> GetInitialListOfServers();

        IReadOnlyList<T> GetUpdatedListOfServers();
    }
}