using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Feign.Codec
{
    public class DefaultDecoder : IDecoder
    {
        public static readonly DefaultDecoder Default = new DefaultDecoder();

        #region Implementation of IDecoder

        /// <inheritdoc/>
        public async Task<object> DecodeAsync(HttpResponseMessage response, Type type)
        {
            var statusCode = (int)response.StatusCode;

            if (statusCode == 404)
            {
                return null;
            }

            if (response.RequestMessage.Method == HttpMethod.Head && type == typeof(bool))
            {
                if (statusCode == 404)
                {
                    return false;
                }
                if (statusCode >= 200 && statusCode < 300)
                {
                    return true;
                }
            }

            response.EnsureSuccessStatusCode();

            if (type == null || type == typeof(Task) || type == typeof(void))
            {
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(json, type);
        }

        #endregion Implementation of IDecoder
    }
}