using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.Ping
{
    public class DummyPing : IPing
    {
        public static DummyPing Default = new DummyPing();

        #region Implementation of IPing

        /// <inheritdoc/>
        public Task<bool> IsAliveAsync(Server server)
        {
            return Task.FromResult(true);
        }

        #endregion Implementation of IPing
    }
}