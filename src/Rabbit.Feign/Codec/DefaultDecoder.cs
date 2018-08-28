using Newtonsoft.Json;
using Steeltoe.CircuitBreaker.Hystrix.Exceptions;
using System;
using System.Diagnostics;
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

            if (type != null && !type.IsValueType && statusCode == 404)
            {
                return null;
            }

            var bodyString = await ReadAsStringAsync(response);

            if (!response.IsSuccessStatusCode && statusCode >= 400 && statusCode < 500)
            {
                throw new HystrixBadRequestException($"Response status code does not indicate success: ${statusCode} ({response.ReasonPhrase}),Content: {bodyString}.");
            }

            response.EnsureSuccessStatusCode();

            if (type == null || type == typeof(Task) || type == typeof(void))
            {
                return null;
            }

            var json = bodyString;
            return JsonConvert.DeserializeObject(json, type);
        }

        #endregion Implementation of IDecoder

        private static async Task<string> ReadAsStringAsync(HttpResponseMessage message)
        {
            if (message?.Content == null)
            {
                return null;
            }

            try
            {
                return await message.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("read string error:" + e.Message);
                return null;
            }
        }
    }
}