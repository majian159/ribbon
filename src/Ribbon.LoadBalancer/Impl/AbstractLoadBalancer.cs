using System.Collections.Generic;

namespace Ribbon.LoadBalancer.Impl
{
    public abstract class AbstractLoadBalancer : ILoadBalancer
    {
        public enum ServerGroup
        {
            All,
            StatusUp,
            StatusNotUp
        }

        #region Implementation of ILoadBalancer

        /// <inheritdoc/>
        public abstract void AddServers(List<Server> newServers);

        /// <inheritdoc/>
        public abstract Server ChooseServer(object key);

        /// <inheritdoc/>
        public abstract void MarkServerDown(Server server);

        /// <inheritdoc/>
        public abstract IReadOnlyList<Server> GetReachableServers();

        /// <inheritdoc/>
        public abstract IReadOnlyList<Server> GetAllServers();

        #endregion Implementation of ILoadBalancer

        public virtual Server ChooseServer()
        {
            return ChooseServer(null);
        }

        public abstract IReadOnlyList<Server> GetServerList(ServerGroup serverGroup);
    }
}