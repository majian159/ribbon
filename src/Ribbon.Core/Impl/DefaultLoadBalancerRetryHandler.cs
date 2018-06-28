using Ribbon.Client.Config;
using Ribbon.Client.Util;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Ribbon.Client.Impl
{
    public class DefaultLoadBalancerRetryHandler : IRetryHandler
    {
        private readonly List<Type> _retriable = new List<Type>
        {
            typeof(TimeoutException),typeof(SocketException)
        };

        private readonly List<Type> _circuitRelated = new List<Type>
        {
            typeof(TimeoutException),typeof(SocketException)
        };

        public DefaultLoadBalancerRetryHandler()
        {
        }

        protected int RetrySameServer { get; set; }

        protected int RetryNextServer { get; set; }

        protected bool RetryEnabled { get; set; }

        /// <inheritdoc/>
        public DefaultLoadBalancerRetryHandler(int retrySameServer, int retryNextServer, bool retryEnabled)
        {
            RetrySameServer = retrySameServer;
            RetryNextServer = retryNextServer;
            RetryEnabled = retryEnabled;
        }

        public DefaultLoadBalancerRetryHandler(IClientConfig clientConfig)
        {
            RetrySameServer = clientConfig.Get(CommonClientConfigKey.MaxAutoRetries, DefaultClientConfigImpl.DEFAULT_MAX_AUTO_RETRIES);
            RetryNextServer = clientConfig.Get(CommonClientConfigKey.MaxAutoRetriesNextServer, DefaultClientConfigImpl.DEFAULT_MAX_AUTO_RETRIES_NEXT_SERVER);
            RetryEnabled = clientConfig.Get(CommonClientConfigKey.OkToRetryOnAllOperations, DefaultClientConfigImpl.DEFAULT_OK_TO_RETRY_ON_ALL_OPERATIONS);
        }

        #region Implementation of IRetryHandler

        /// <inheritdoc/>
        public bool IsRetriableException(Exception exception, bool sameServer)
        {
            return RetryEnabled && (!sameServer || Utils.IsPresentAsCause(exception, GetRetriableExceptions()));
        }

        /// <inheritdoc/>
        public bool IsCircuitTrippingException(Exception exception)
        {
            return Utils.IsPresentAsCause(exception, GetCircuitRelatedExceptions());
        }

        /// <inheritdoc/>
        public int GetMaxRetriesOnSameServer()
        {
            return RetrySameServer;
        }

        /// <inheritdoc/>
        public int GetMaxRetriesOnNextServer()
        {
            return RetryNextServer;
        }

        #endregion Implementation of IRetryHandler

        protected virtual IReadOnlyCollection<Type> GetRetriableExceptions()
        {
            return _retriable;
        }

        protected virtual IReadOnlyCollection<Type> GetCircuitRelatedExceptions()
        {
            return _circuitRelated;
        }
    }
}