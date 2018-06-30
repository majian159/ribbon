using Consul;
using Ribbon.LoadBalancer;
using Steeltoe.Discovery.Consul.Util;
using System.Linq;

namespace Ribbon.Consul
{
    public class ConsulServer : Server
    {
        private readonly ServiceEntry _serviceEntry;

        public ConsulServer(ServiceEntry serviceEntry) : base(ConsulServerUtils.FindHost(serviceEntry), serviceEntry.Service.Port)
        {
            _serviceEntry = serviceEntry;
            MetaInfo = new ConsulMetaInfo(serviceEntry);
        }

        #region Overrides of Server

        /// <inheritdoc/>
        public override IMetaInfo MetaInfo { get; }

        #endregion Overrides of Server

        public bool IsPassingChecks()
        {
            return _serviceEntry.Checks.All(i => Equals(i.Status, HealthStatus.Passing));
        }

        private class ConsulMetaInfo : IMetaInfo
        {
            public ConsulMetaInfo(ServiceEntry serviceEntry)
            {
                AppName = serviceEntry.Service.Service;
                var metadata = ConsulServerUtils.GetMetadata(serviceEntry);
                ServerGroup = metadata.TryGetValue("group", out var value) ? value : null;
                ServiceIdForDiscovery = null;
                InstanceId = serviceEntry.Service.ID;
            }

            #region Implementation of IMetaInfo

            /// <inheritdoc/>
            public string AppName { get; }

            /// <inheritdoc/>
            public string ServerGroup { get; }

            /// <inheritdoc/>
            public string ServiceIdForDiscovery { get; }

            /// <inheritdoc/>
            public string InstanceId { get; }

            #endregion Implementation of IMetaInfo
        }
    }
}