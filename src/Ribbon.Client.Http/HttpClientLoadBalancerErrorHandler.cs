using Ribbon.Client.Impl;
using Ribbon.Client.Util;
using System;
using System.Net.Sockets;

namespace Ribbon.Client.Http
{
    public class HttpClientLoadBalancerErrorHandler : DefaultLoadBalancerRetryHandler
    {
        public HttpClientLoadBalancerErrorHandler(RetryHandlerConfig options) : base(options)
        {
        }

        #region Overrides of DefaultLoadBalancerRetryHandler

        public override bool IsRetriableException(Exception exception, bool sameServer)
        {
            if (exception is ClientException clientException)
            {
                if (clientException.Type == ClientException.ErrorType.ServerThrottled)
                {
                    return !sameServer && RetryEnabled;
                }
            }

            var socketException = Utils.GetInnerException<SocketException>(exception);

            if (socketException != null)
            {
                switch (socketException.SocketErrorCode)
                {
                    //ConnectException
                    case SocketError.ConnectionRefused:
                    //SocketTimeoutException ConnectTimeoutException
                    case SocketError.TimedOut:
                    case SocketError.Shutdown:
                        return true;

                    default:
                        return false;
                }
            }

            return false;
        }

        public override bool IsCircuitTrippingException(Exception exception)
        {
            if (exception is ClientException clientException)
            {
                return clientException.Type == ClientException.ErrorType.ServerThrottled;
            }

            var socketException = Utils.GetInnerException<SocketException>(exception);

            return socketException != null;
        }

        #endregion Overrides of DefaultLoadBalancerRetryHandler
    }
}