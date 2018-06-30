using System;

namespace Ribbon.LoadBalancer
{
    public class Server
    {
        private string _id;
        private string _host;
        private int _port;

        public Server(string host, int port) : this(null, host, port)
        {
        }

        public Server(string scheme, string host, int port) : this()
        {
            Scheme = scheme;
            _host = host;
            _port = port;
            _id = $"{host}:{port}";
        }

        public Server(string id) : this()
        {
            Id = id;
        }

        private Server()
        {
            IsAlive = false;
            MetaInfo = new SimpleMetaInfo(this);
            IsReadyToServe = true;
        }

        public interface IMetaInfo
        {
            string AppName { get; }
            string ServerGroup { get; }
            string ServiceIdForDiscovery { get; }

            string InstanceId { get; }
        }

        private class SimpleMetaInfo : IMetaInfo
        {
            private readonly Server _server;

            public SimpleMetaInfo(Server server)
            {
                _server = server;
            }

            #region Implementation of IMetaInfo

            /// <inheritdoc/>
            public string AppName => null;

            /// <inheritdoc/>
            public string ServerGroup => null;

            /// <inheritdoc/>
            public string ServiceIdForDiscovery => null;

            /// <inheritdoc/>
            public string InstanceId => _server.Id;

            #endregion Implementation of IMetaInfo
        }

        public bool IsAlive { get; set; }
        public bool IsReadyToServe { get; set; }

        public string Id
        {
            get => _id;
            set
            {
                var result = GetSchemeHostPort(value);
                if (result == null)
                {
                    _id = null;
                    return;
                }

                var (scheme, host, port) = result.Value;
                _id = $"{host}:{port}";
                Host = host;
                Port = port;
                Scheme = scheme;
            }
        }

        public string Scheme { get; set; }

        public string Host
        {
            get => _host;
            set
            {
                if (value == null)
                {
                    return;
                }

                _host = value;

                _id = $"{value}:{Port}";
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                _port = value;

                if (_host != null)
                {
                    _id = $"{_host}:{value}";
                }
            }
        }

        public virtual IMetaInfo MetaInfo { get; }

        private static (string Scheme, string Host, int Port)? GetSchemeHostPort(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            string scheme = null;
            string host;
            var port = 80;

            if (id.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "http";
                id = id.Substring(7);
                port = 80;
            }
            else if (id.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                scheme = "https";
                id = id.Substring(8);
                port = 443;
            }

            if (id.Contains("/"))
            {
                id = id.Substring(0, id.IndexOf('/'));
            }

            var colonIdx = id.IndexOf(':');
            if (colonIdx == -1)
            {
                host = id;
            }
            else
            {
                host = id.Substring(0, colonIdx);
                port = int.Parse(id.Substring(colonIdx + 1));
            }

            return (scheme, host, port);
        }

        #region Overrides of Object

        /// <inheritdoc/>
        public override string ToString()
        {
            return _id;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is Server svc))
            {
                return false;
            }

            return svc.Id == Id;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 7;
                hash = 31 * hash + (null == Id ? 0 : Id.GetHashCode());
                return hash;
            }
        }

        #endregion Overrides of Object
    }

    public static class ServerExtensions
    {
        public static string GetHostPort(this Server server)
        {
            return $"{server.Host}:{server.Port}";
        }
    }
}