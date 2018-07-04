using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;

namespace Ribbon.LoadBalancer.ConsulDiscovery
{
    public class HealthServiceServerListFilter : IServerListFilter<Server>
    {
        private readonly ILogger<HealthServiceServerListFilter> _logger;

        public HealthServiceServerListFilter(ILogger<HealthServiceServerListFilter> logger)
        {
            _logger = logger;
        }

        public HealthServiceServerListFilter()
        {
            _logger = NullLogger<HealthServiceServerListFilter>.Instance;
        }

        #region Implementation of IServerListFilter<Server>

        /// <inheritdoc/>
        public Server[] GetFilteredListOfServers(Server[] servers)
        {
            var filtered = new List<Server>();

            foreach (var server in servers)
            {
                if (server is ConsulServer consulServer)
                {
                    if (consulServer.IsPassingChecks())
                    {
                        filtered.Add(server);
                    }
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Unable to determine aliveness of server type " + server.GetType() + ", " + server);
                    }
                    filtered.Add(server);
                }
            }

            return filtered.ToArray();
        }

        #endregion Implementation of IServerListFilter<Server>
    }
}