using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Ribbon.Client.Http
{
    public class RibbonHttpClient : IClient
    {
        public HttpClient HttpClient { get; }
        private readonly RibbonHttpClientOptions _options;

        public RibbonHttpClient(string name, IOptionsMonitor<RibbonHttpClientOptions> optionsMonitor)
        {
            _options = optionsMonitor.Get(name);
            HttpClient = _options.HttpClient;
        }

        #region Implementation of IClient

        /// <inheritdoc/>
        public Task<object> ExecuteAsync(object request, CancellationToken cancellationToken)
        {
            return ExecuteAsync(request, _options.LoadBalancerClientOptions.DefaultExecuteOptions, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(object request, ExecuteOptions settings, CancellationToken cancellationToken)
        {
            if (request is HttpRequest httpRequest)
            {
                var requestMessage = CreateHttpRequestMessage(httpRequest);

                var responseMessage = await HttpClient.SendAsync(requestMessage, cancellationToken);
                return new HttpResponse(responseMessage);
            }

            throw new ArgumentException($"request type not {typeof(HttpRequest)}.", nameof(request));
        }

        #endregion Implementation of IClient

        private static HttpRequestMessage CreateHttpRequestMessage(HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage(request.Method, request.Uri)
            {
                Content = request.Content
            };

            if (request.HttpHeaders != null)
            {
                foreach (var header in request.HttpHeaders)
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return requestMessage;
        }

        private class HttpResponse : IHttpResponse
        {
            private readonly HttpResponseMessage _responseMessage;

            public HttpResponse(HttpResponseMessage responseMessage)
            {
                _responseMessage = responseMessage;
                HasContent = responseMessage.Content != null;
                Content = HasContent ? responseMessage.Content : null;

                HasBody = Content?.Headers.ContentLength != null && Content.Headers.ContentLength.Value > 0;
                Body = HasBody ? Content?.ReadAsStreamAsync().GetAwaiter().GetResult() : null;

                IsSuccess = responseMessage.IsSuccessStatusCode;
                RequestedUri = responseMessage.RequestMessage.RequestUri;
                Headers = responseMessage.Headers.ToDictionary(i => i.Key, i => (object)string.Join(",", i.Value));
                Status = (int)responseMessage.StatusCode;
                StatusLine = $"{responseMessage.Version} {(int)responseMessage.StatusCode} {responseMessage.ReasonPhrase}";
                HttpHeaders = responseMessage.Headers;
                InputStream = HasBody ? (Stream)Body : Stream.Null;
            }

            #region Implementation of IDisposable

            /// <inheritdoc/>
            public void Dispose()
            {
                _responseMessage.Dispose();
            }

            #endregion Implementation of IDisposable

            #region Implementation of IResponse

            /// <inheritdoc/>
            public object Body { get; }

            /// <inheritdoc/>
            public bool HasBody { get; }

            /// <inheritdoc/>
            public bool IsSuccess { get; }

            /// <inheritdoc/>
            public Uri RequestedUri { get; }

            /// <inheritdoc/>
            public IDictionary<string, object> Headers { get; }

            #endregion Implementation of IResponse

            #region Implementation of IHttpResponse

            /// <inheritdoc/>
            public int Status { get; }

            /// <inheritdoc/>
            public string StatusLine { get; }

            /// <inheritdoc/>
            public HttpHeaders HttpHeaders { get; }

            /// <inheritdoc/>
            public Stream InputStream { get; }

            /// <inheritdoc/>
            public bool HasContent { get; }

            /// <inheritdoc/>
            public HttpContent Content { get; }

            #endregion Implementation of IHttpResponse
        }
    }
}