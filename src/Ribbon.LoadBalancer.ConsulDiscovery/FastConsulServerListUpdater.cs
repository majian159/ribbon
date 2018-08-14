using Consul;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ribbon.LoadBalancer.Impl.ServerList;
using Steeltoe.Discovery.Consul.Discovery;
using System;
using System.Threading.Tasks;

namespace Ribbon.LoadBalancer.ConsulDiscovery
{
    public class FastConsulServerListUpdater : PollingServerListUpdater
    {
        private readonly string _clientName;
        private readonly ConsulDiscoveryOptions _consulDiscoveryOptions;
        private readonly ConsulClient _consulClient;
        private ILogger<FastConsulServerListUpdater> _logger;

        /// <inheritdoc/>
        public FastConsulServerListUpdater(string clientName, IServiceProvider services, LoadBalancerConfig config, ILogger logger) : base(config, logger)
        {
            _clientName = clientName;
            _logger = services.GetRequiredService<ILogger<FastConsulServerListUpdater>>();
            var consulDiscoveryOptionsAccessor = services.GetRequiredService<IOptions<ConsulDiscoveryOptions>>();
            _consulClient = services.GetRequiredService<ConsulClient>();
            _consulDiscoveryOptions = consulDiscoveryOptionsAccessor.Value;
        }

        #region Overrides of PollingServerListUpdater

        /// <inheritdoc/>
        public override void Start(Func<Task> updateAction)
        {
            base.Start(updateAction);

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(2000);
                ulong lastIndex = 0;
                while (true)
                {
                    if (Processing)
                    {
                        continue;
                    }

                    var response = await _consulClient.Health.Service(_clientName, _consulDiscoveryOptions.DefaultQueryTag,
                        _consulDiscoveryOptions.QueryPassing,
                        new QueryOptions
                        {
                            WaitIndex = lastIndex
                        });

                    if (response.LastIndex == lastIndex)
                    {
                        continue;
                    }

                    lastIndex = response.LastIndex;

                    Processing = true;
                    try
                    {
                        await UpdateAction();
                    }
                    finally
                    {
                        Processing = false;
                    }
                }
            });
        }

        #endregion Overrides of PollingServerListUpdater
    }
}