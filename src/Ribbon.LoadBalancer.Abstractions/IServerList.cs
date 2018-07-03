using System.Threading.Tasks;

namespace Ribbon.LoadBalancer
{
    public interface IServerList<T> where T : Server
    {
        Task<T[]> GetInitialListOfServersAsync();

        Task<T[]> GetUpdatedListOfServersAsync();
    }
}