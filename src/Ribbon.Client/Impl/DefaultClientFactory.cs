using Microsoft.Extensions.Options;
using System;

namespace Ribbon.Client.Impl
{
    public class DefaultClientFactory : IClientFactory
    {
        private readonly IServiceProvider _services;
        private readonly IOptionsMonitor<LoadBalancerClientOptions> _clientOptionsMonitor;

        public DefaultClientFactory(IServiceProvider services, IOptionsMonitor<LoadBalancerClientOptions> clientOptionsMonitor)
        {
            _services = services;
            _clientOptionsMonitor = clientOptionsMonitor;
        }

        #region Implementation of IClientFactory

        /// <inheritdoc/>
        public IClient CreateClient(string name)
        {
            var clientOptions = _clientOptionsMonitor.Get(name);
            return clientOptions.Creator(name, _services);
        }

        #endregion Implementation of IClientFactory
    }
}