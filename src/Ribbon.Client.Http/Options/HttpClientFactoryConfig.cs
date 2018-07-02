using System;

namespace Ribbon.Client.Http.Options
{
    public class HttpClientFactoryConfig
    {
        public static readonly HttpClientFactoryConfig Default = new HttpClientFactoryConfig();

        public HttpClientFactoryConfig()
        {
            Timeout = TimeSpan.FromSeconds(10);
        }

        public TimeSpan Timeout { get; set; }
    }
}