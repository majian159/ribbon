using Ribbon.Client;
using Ribbon.Client.Impl;
using System;
using System.Text;

namespace Ribbon.LoadBalancer
{
    public class LoadBalancerContext
    {
        public string ClientName { get; protected set; }
        public ILoadBalancer LoadBalancer { get; set; }
        public IRetryHandler RetryHandler { get; set; }

        public LoadBalancerContext(ILoadBalancer loadBalancer) : this(loadBalancer, null)
        {
        }

        public LoadBalancerContext(ILoadBalancer loadBalancer, IRetryHandler retryHandler)
        {
            LoadBalancer = loadBalancer;
            RetryHandler = retryHandler ?? new DefaultLoadBalancerRetryHandler();
        }

        public Uri ReconstructUriWithServer(Server server, Uri originalUri)
        {
            var host = server.Host;
            var port = server.Port;
            var scheme = server.Scheme;

            if (originalUri.IsAbsoluteUri && string.Equals(scheme, originalUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(host, originalUri.Host, StringComparison.OrdinalIgnoreCase) && port == server.Port)
            {
                return originalUri;
            }

            var sb = new StringBuilder();
            sb.Append(scheme).Append("://");

            if (originalUri.IsAbsoluteUri && !string.IsNullOrEmpty(originalUri.UserInfo))
            {
                sb.Append(originalUri.UserInfo).Append("@");
            }

            sb.Append(host);

            if (port >= 0)
            {
                sb.Append(":").Append(port);
            }

            sb.Append(originalUri.PathAndQuery);

            if (!string.IsNullOrEmpty(originalUri.Fragment))
            {
                sb.Append(originalUri.Fragment);
            }

            return new Uri(sb.ToString());
        }
    }
}