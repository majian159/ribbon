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

        protected uint RetrySameServer { get; set; }

        protected uint RetryNextServer { get; set; }

        protected bool RetryEnabled { get; set; }

        /// <inheritdoc/>
        public DefaultLoadBalancerRetryHandler(uint retrySameServer, uint retryNextServer, bool retryEnabled)
        {
            RetrySameServer = retrySameServer;
            RetryNextServer = retryNextServer;
            RetryEnabled = retryEnabled;
        }

        public DefaultLoadBalancerRetryHandler(RetryHandlerConfig options)
        {
            RetrySameServer = options.MaxAutoRetries;
            RetryNextServer = options.MaxAutoRetriesNextServer;
            RetryEnabled = options.OkToRetryOnAllOperations;
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
        public uint GetMaxRetriesOnSameServer()
        {
            return RetrySameServer;
        }

        /// <inheritdoc/>
        public uint GetMaxRetriesOnNextServer()
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