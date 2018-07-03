using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Ribbon.LoadBalancer.Impl.Ping;
using Ribbon.LoadBalancer.Impl.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.Impl
{
    public class BaseLoadBalancer : AbstractLoadBalancer
    {
        private readonly ILoadBalancerSettings _settings;
        private readonly ILogger _logger;
        private static readonly IRule DefaultRule = new RoundRobinRule();
        private const string DefaultName = "default";

        private IRule _rule;

        protected IRule Rule
        {
            get => _rule;
            set
            {
                _rule = value ?? new RoundRobinRule();

                if (_rule.LoadBalancer != this)
                {
                    _rule.LoadBalancer = this;
                }
            }
        }

        protected IPing Ping { get; set; }

        protected volatile Server[] AllServerList = Enumerable.Empty<Server>().ToArray();
        protected volatile Server[] UpServerList = Enumerable.Empty<Server>().ToArray();

        protected ReaderWriterLockSlim AllServerLock { get; } = new ReaderWriterLockSlim();
        protected ReaderWriterLockSlim UpServerLock { get; } = new ReaderWriterLockSlim();

        protected string Name { get; set; }
        protected Timer PingTimer { get; private set; }
        protected TimeSpan PingInterval { get; set; }

        public BaseLoadBalancer(ILogger logger)
        {
            Name = DefaultName;
            Ping = null;
            Rule = DefaultRule;
            SetupPingTask();
            _logger = logger ?? NullLogger.Instance;
        }

        public BaseLoadBalancer(string name, IRule rule, IPing ping, ILoadBalancerSettings settings, ILogger logger = null)
        {
            _settings = settings;
            _logger = logger ?? NullLogger.Instance;
            Name = name;
            Ping = ping;
            Rule = rule;
            SetupPingTask();
        }

        #region Overrides of AbstractLoadBalancer

        /// <inheritdoc/>
        public override void AddServers(List<Server> newServers)
        {
            if (newServers == null || !newServers.Any())
            {
                return;
            }

            AllServerLock.EnterReadLock();
            Server[] servers;
            try
            {
                servers = AllServerList.Concat(newServers).ToArray();
            }
            finally
            {
                AllServerLock.ExitReadLock();
            }

            SetServersList(servers);
        }

        /// <inheritdoc/>
        public override Server ChooseServer(object key)
        {
            if (Rule == null)
            {
                return null;
            }

            try
            {
                return Rule.Choose(key);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, $"LoadBalancer [{Name}]:  Error choosing server for key {key}");
                return null;
            }
        }

        /// <inheritdoc/>
        public override void MarkServerDown(Server server)
        {
            if (server == null || !server.IsAlive)
            {
                return;
            }

            _logger.LogError($"LoadBalancer [{Name}]:  markServerDown called on [{server.Id}]");
            server.IsAlive = false;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Server> GetReachableServers()
        {
            return UpServerList;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Server> GetAllServers()
        {
            return AllServerList;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<Server> GetServerList(ServerGroup serverGroup)
        {
            switch (serverGroup)
            {
                case ServerGroup.All:
                    return AllServerList;

                case ServerGroup.StatusUp:
                    return UpServerList;

                case ServerGroup.StatusNotUp:
                    {
                        return AllServerList.Except(UpServerList).ToArray();
                    }
            }

            return Enumerable.Empty<Server>().ToArray();
        }

        #endregion Overrides of AbstractLoadBalancer

        #region Public Method

        public void ForceQuickPing()
        {
            if (CanSkipPing())
            {
                return;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"LoadBalancer [{Name}]:  forceQuickPing invoking");
            }

            PingTask();
        }

        #endregion Public Method

        #region Protected Method

        protected void SetServersList(Server[] servers)
        {
            if (servers == null)
            {
                throw new ArgumentNullException(nameof(servers));
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug($"LoadBalancer [{Name}]: clearing server list (SET op)");
            }

            AllServerLock.EnterWriteLock();
            try
            {
                AllServerList = servers;
                if (CanSkipPing())
                {
                    foreach (var server in servers)
                    {
                        server.IsAlive = true;
                    }

                    UpServerList = AllServerList;
                }
            }
            finally
            {
                AllServerLock.ExitWriteLock();
            }
        }

        #endregion Protected Method

        #region Private Method

        private bool CanSkipPing()
        {
            return Ping == null || Ping is DummyPing;
        }

        private void PingTask()
        {
            if (CanSkipPing())
            {
                return;
            }

            try
            {
                AllServerLock.EnterReadLock();

                var newUpList = new List<Server>();
                Parallel.ForEach(AllServerList, server =>
                {
                    if (!Ping.IsAliveAsync(server).GetAwaiter().GetResult())
                    {
                        return;
                    }

                    newUpList.Add(server);
                });

                UpServerLock.EnterWriteLock();
                UpServerList = newUpList.ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"LoadBalancer[{Name}]: Error pinging");
            }
            finally
            {
                AllServerLock.ExitReadLock();
                UpServerLock.ExitWriteLock();
            }
        }

        private void SetupPingTask()
        {
            if (CanSkipPing())
            {
                return;
            }

            PingTask();

            PingInterval = _settings.LoadBalancerPingInterval;

            var pingProcessing = false;
            PingTimer = new Timer(s =>
            {
                if (pingProcessing)
                {
                    return;
                }

                pingProcessing = true;

                try
                {
                    PingTask();
                }
                finally
                {
                    pingProcessing = false;
                }
            }, null, TimeSpan.Zero, PingInterval);
        }

        #endregion Private Method
    }
}