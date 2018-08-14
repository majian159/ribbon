using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.ServerList
{
    public class PollingServerListUpdater : IServerListUpdater
    {
        private readonly LoadBalancerConfig _config;
        private readonly Timer _timer;
        protected Func<Task> UpdateAction { get; private set; }

        protected bool Processing { get; set; }

        public PollingServerListUpdater(LoadBalancerConfig config, ILogger logger)
        {
            _config = config;
            _timer = new Timer(async s =>
            {
                if (UpdateAction == null || Processing)
                {
                    return;
                }

                Processing = true;

                try
                {
                    await UpdateAction();
                }
                catch (Exception e)
                {
                    logger.LogWarning(e, "Failed one update cycle");
                }
                finally
                {
                    Processing = false;
                }
            }, null, -1, -1);
        }

        #region Implementation of IServerListUpdater

        /// <inheritdoc/>
        public virtual void Start(Func<Task> updateAction)
        {
            UpdateAction = updateAction;

            if (updateAction == null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                _timer.Change(TimeSpan.FromSeconds(1), _config?.ServerListRefreshInterval ?? TimeSpan.FromSeconds(30));
            }
        }

        /// <inheritdoc/>
        public virtual void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #endregion Implementation of IServerListUpdater
    }
}