/*using Ribbon.Client.Impl;
using Ribbon.Client.Util;
using Ribbon.LoadBalancer;
using Ribbon.LoadBalancer.Util;
using System;
using System.Net.Sockets;

namespace Ribbon.Client
{
    public class LoadBalancerContext
    {
        public string ClientName { get; protected set; }
        public ILoadBalancer LoadBalancer { get; set; }
        public IRetryHandler RetryHandler { get; set; }

        public LoadBalancerContext(string clientName, ILoadBalancer loadBalancer) : this(clientName, loadBalancer, null)
        {
        }

        public LoadBalancerContext(string clientName, ILoadBalancer loadBalancer, IRetryHandler retryHandler)
        {
            ClientName = clientName;
            LoadBalancer = loadBalancer;
            RetryHandler = retryHandler ?? new DefaultLoadBalancerRetryHandler(null);
        }

        public static Uri ReconstructUriWithServer(Server server, Uri originalUri)
        {
            return LoadBalancerUtil.ReconstructUriWithServer(server, originalUri);
        }

        public static ClientException GenerateException(string uri, Exception e)
        {
            var message = $"Unable to execute RestClient request for URI:{uri}";

            if (e is ClientException clientException)
            {
                return clientException;
            }

            var socketException = Utils.GetInnerException<SocketException>(e);
            if (socketException != null)
            {
                switch (socketException.SocketErrorCode)
                {
                    case SocketError.TimedOut:
                        return new ClientException(ClientException.ErrorType.TimeoutException,
                            $"{message}:{e.GetBaseException().Message}", e);

                    case SocketError.HostNotFound:
                        return new ClientException(ClientException.ErrorType.UnknownHostException,
                            message, e);

                    case SocketError.ConnectionReset:
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionRefused:
                        return new ClientException(ClientException.ErrorType.ConnectException,
                            message, e);

                    case SocketError.HostUnreachable:
                        return new ClientException(ClientException.ErrorType.NoRouteToHostException,
                            message, e);
                }
            }

            return new ClientException(ClientException.ErrorType.General, message, e);
        }
    }
}*/