using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl.ServerList
{
    public class PollingServerListUpdater : IServerListUpdater
    {
        private readonly LoadBalancerConfig _config;
        private readonly Timer _timer;
        private Func<Task> _updateAction;

        public PollingServerListUpdater(LoadBalancerConfig config)
        {
            _config = config;
            _timer = new Timer(async s =>
            {
                if (_updateAction == null)
                {
                    return;
                }

                await _updateAction();
            }, null, -1, -1);
        }

        #region Implementation of IServerListUpdater

        /// <inheritdoc/>
        public void Start(Func<Task> updateAction)
        {
            _updateAction = updateAction;
            if (updateAction == null)
            {
                _timer.Change(-1, -1);
            }
            else
            {
                _timer.Change(TimeSpan.FromSeconds(1), _config?.ServerListRefreshInterval ?? TimeSpan.FromSeconds(30));
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            _timer.Change(TimeSpan.MinValue, TimeSpan.MinValue);
        }

        #endregion Implementation of IServerListUpdater
    }
}