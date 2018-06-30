using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer
{
    public interface IServerList<T> where T : Server
    {
        Task<IReadOnlyList<T>> GetInitialListOfServersAsync();

        Task<IReadOnlyList<T>> GetUpdatedListOfServersAsync();
    }
}