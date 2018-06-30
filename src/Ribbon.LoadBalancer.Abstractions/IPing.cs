using System.Threading.Tasks;

namespace Ribbon.LoadBalancer
{
    public interface IPing
    {
        Task<bool> IsAliveAsync(Server server);
    }
}