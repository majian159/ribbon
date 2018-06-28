using System;
using System.Collections.Generic;

namespace Ribbon.Client.Config
{
    public class DefaultClientConfig : IClientConfig
    {
        #region Implementation of IClientConfig

        /// <inheritdoc/>
        public string ClientName { get; set; }

        /// <inheritdoc/>
        public string ClientNameSpace { get; set; }

        /// <inheritdoc/>
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        #endregion Implementation of IClientConfig
    }
}