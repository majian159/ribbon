using System;
using System.Linq;
using System.Threading;

namespace Ribbon.LoadBalancer.Impl.Rule
{
    public class RandomRule : IRule
    {
        private readonly ThreadLocal<Random> _randomLocal = new ThreadLocal<Random>(() => new Random());

        #region Implementation of IRule

        /// <inheritdoc/>
        public ILoadBalancer LoadBalancer { get; set; }

        /// <inheritdoc/>
        public Server Choose(object key)
        {
            var lb = LoadBalancer;

            if (lb == null)
            {
                return null;
            }

            while (true)
            {
                var reachableServers = lb.GetReachableServers();
                var serverCount = reachableServers.Count;

                if (serverCount == 0)
                {
                    return null;
                }

                Server server;
                if (serverCount == 1)
                {
                    server = reachableServers[0];
                }
                else
                {
                    var index = ChooseRandomInt(serverCount);
                    server = reachableServers.ElementAtOrDefault(index);
                }

                if (server == null)
                {
                    Thread.Yield();
                    continue;
                }

                if (server.IsAlive)
                {
                    return server;
                }

                Thread.Yield();
            }
        }

        #endregion Implementation of IRule

        protected int ChooseRandomInt(int serverCount)
        {
            return _randomLocal.Value.Next(0, serverCount);
        }
    }
}