using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.Ping
{
    public class NoOpPing : IPing
    {
        public static readonly IPing Default = new NoOpPing();

        #region Implementation of IPing

        /// <inheritdoc/>
        public Task<bool> IsAliveAsync(Server server)
        {
            return Task.FromResult(true);
        }

        #endregion Implementation of IPing
    }
}