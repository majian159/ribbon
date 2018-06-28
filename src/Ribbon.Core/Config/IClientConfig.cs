using System.Collections.Generic;

namespace Ribbon.Client.Config
{
    public interface IClientConfig
    {
        string ClientName { get; }
        string ClientNameSpace { get; }

        IDictionary<string, object> Properties { get; }
    }

    public static class ClientConfigExtensions
    {
        public static T Get<T>(this IClientConfig client, string key)
        {
            return client.Get(key, default(T));
        }

        public static T Get<T>(this IClientConfig client, string key, T defaultValue)
        {
            if (!client.Properties.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            return (T)value;
        }
    }
}