using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Consul
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