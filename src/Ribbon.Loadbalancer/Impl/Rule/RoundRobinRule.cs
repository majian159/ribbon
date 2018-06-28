using System.Threading;

namespace Ribbon.LoadBalancer.Impl.Rule
{
    public class RoundRobinRule : IRule
    {
        private long _nextServerCyclicCounter;

        public RoundRobinRule()
        {
        }

        public RoundRobinRule(ILoadBalancer loadBalancer)
        {
            LoadBalancer = loadBalancer;
        }

        #region Implementation of IRule

        /// <inheritdoc/>
        public ILoadBalancer LoadBalancer { get; set; }

        /// <inheritdoc/>
        public Server Choose(object key)
        {
            if (LoadBalancer == null)
            {
                return null;
            }

            var lb = LoadBalancer;

            var count = 0;
            while (count++ < 10)
            {
                var reachableServers = lb.GetReachableServers();
                var allServers = lb.GetAllServers();
                var upCount = reachableServers.Count;
                var serverCount = allServers.Count;

                if (upCount == 0 || serverCount == 0)
                {
                    return null;
                }

                var nextServerIndex = IncrementAndGetModulo(serverCount);
                var server = allServers[nextServerIndex];

                if (server == null)
                {
                    Thread.Yield();
                    continue;
                }

                if (server.IsAlive && server.IsReadyToServe)
                {
                    return server;
                }
            }

            return null;
        }

        #endregion Implementation of IRule

        private int IncrementAndGetModulo(int modulo)
        {
            while (true)
            {
                var current = Interlocked.Read(ref _nextServerCyclicCounter);

                var next = (current + 1) % modulo;
                if (Interlocked.CompareExchange(ref _nextServerCyclicCounter, next, current) == current)
                {
                    return (int)next;
                }
            }
        }
    }
}