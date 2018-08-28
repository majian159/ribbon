using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Feign.Codec
{
    public class DefaultEncoder : IEncoder
    {
        #region Implementation of IEncoder

        /// <inheritdoc/>
        public Task EncodeAsync(object obj, Type bodyType, HttpRequestMessage request)
        {
            if (obj == null)
            {
                return Task.CompletedTask;
            }

            var json = JsonConvert.SerializeObject(obj);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType.MediaType = "application/json";

            return Task.CompletedTask;
        }

        #endregion Implementation of IEncoder
    }
}