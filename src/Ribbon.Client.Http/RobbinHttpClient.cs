using Ribbon.LoadBalancer;
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
    public class RobbinHttpClient : LoadBalancerContext, IClient
    {
        private readonly HttpClient _httpClient;

        /// <inheritdoc/>
        public RobbinHttpClient(HttpClient httpClient, ILoadBalancer loadBalancer, IRetryHandler retryHandler) : base(loadBalancer, retryHandler)
        {
            _httpClient = httpClient;
        }

        public RobbinHttpClient(RobbinHttpClientOptions options) : this(options.HttpClient, options.LoadBalancer, options.RetryHandler)
        {
        }

        #region Implementation of IClient

        /// <inheritdoc/>
        public async Task<object> ExecuteAsync(object request, ExecuteOptions settings, CancellationToken cancellationToken)
        {
            return await ExecuteAsync(request as HttpRequest, settings, cancellationToken);
        }

        #endregion Implementation of IClient

        private async Task<IHttpResponse> ExecuteAsync(HttpRequest request, ExecuteOptions executeOptions, CancellationToken cancellationToken)
        {
            var retryHandler = RetryHandler;

            var maxRetriesOnNextServer = retryHandler.GetMaxRetriesOnNextServer();
            var maxRetriesOnSameServer = retryHandler.GetMaxRetriesOnSameServer();

            do
            {
                maxRetriesOnNextServer--;

                var server = LoadBalancer.ChooseServer(null);

                do
                {
                    maxRetriesOnSameServer--;

                    try
                    {
                        await DoExecuteAsync(server, request, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        if (retryHandler.IsCircuitTrippingException(e) || !retryHandler.IsRetriableException(e, true))
                        {
                            throw;
                        }
                        Console.WriteLine(e);
                    }
                } while (maxRetriesOnSameServer > 0);
            } while (maxRetriesOnNextServer > 0);

            return null;
        }

        private async Task<IHttpResponse> DoExecuteAsync(Server server, HttpRequest request, CancellationToken cancellationToken)
        {
            var uri = ReconstructUriWithServer(server, request.Uri);
            var requestMessage = new HttpRequestMessage(request.Method, uri)
            {
                Content = request.Content
            };
            if (request.HttpHeaders != null)
            {
                foreach (var header in request.HttpHeaders)
                {
                    requestMessage.Headers.Remove(header.Key);
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);

            return new HttpResponse(responseMessage);
        }

        private class HttpResponse : IHttpResponse
        {
            private readonly HttpResponseMessage _responseMessage;

            public HttpResponse(HttpResponseMessage responseMessage)
            {
                _responseMessage = responseMessage;
                HasContent = _responseMessage.Content != null;
                Content = HasContent ? _responseMessage.Content : null;

                HasBody = Content?.Headers.ContentLength != null && Content.Headers.ContentLength.Value > 0;
                Body = HasBody ? Content?.ReadAsStreamAsync().GetAwaiter().GetResult() : null;

                IsSuccess = _responseMessage.IsSuccessStatusCode;
                RequestedUri = _responseMessage.RequestMessage.RequestUri;
                Headers = _responseMessage.Headers.ToDictionary(i => i.Key, i => (object)string.Join(",", i.Value));
                Status = (int)_responseMessage.StatusCode;
                StatusLine = $"{_responseMessage.Version} {(int)_responseMessage.StatusCode} {_responseMessage.ReasonPhrase}";
                HttpHeaders = _responseMessage.Headers;
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