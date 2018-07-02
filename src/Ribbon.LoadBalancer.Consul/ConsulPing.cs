using Ribbon.LoadBalancer;
using System.Threading.Tasks;

namespace Ribbon.Consul
{
    public class ConsulPing : IPing
    {
        #region Implementation of IPing

        /// <inheritdoc/>
        public Task<bool> IsAliveAsync(Server server)
        {
            var isAlive = true;
            if (server != null && server is ConsulServer consulServer)
            {
                isAlive = consulServer.IsPassingChecks();
            }

            return Task.FromResult(isAlive);
        }

        #endregion Implementation of IPing
    }
}