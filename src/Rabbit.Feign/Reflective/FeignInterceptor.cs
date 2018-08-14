using Castle.DynamicProxy;
using Rabbit.Feign.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabbit.Feign.Reflective
{
    public class FeignInterceptor : IInterceptor
    {
        private readonly ITemplateParser _templateParser;
        private readonly ClientDescriptor _clientDescriptor;
        private readonly HttpClient _httpClient;
        private readonly IParameterExpanderLocator _parameterExpanderLocator;

        public FeignInterceptor(ClientDescriptor clientDescriptor, IHttpClientFactory httpClientFactory, IParameterExpanderLocator parameterExpanderLocator)
        {
            _templateParser = new TemplateParser();
            _clientDescriptor = clientDescriptor;
            _parameterExpanderLocator = parameterExpanderLocator;
            _httpClient = httpClientFactory.CreateClient(clientDescriptor.Name);
        }

        #region Implementation of IInterceptor

        /// <inheritdoc/>
        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                if (returnType.IsGenericType)
                {
                    var result = GetHandler(invocation.Method.ReturnType)(invocation);
                    invocation.ReturnValue = result;
                }
                else
                {
                    invocation.ReturnValue = DoHandleAsync(invocation);
                }
            }
            else
            {
                invocation.ReturnValue = DoHandleAsync(invocation).GetAwaiter().GetResult();
            }
        }

        #endregion Implementation of IInterceptor

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var result = await DoHandleAsync(invocation);

            if (result == null)
            {
                return default(T);
            }

            return (T)result;
        }

        private readonly ConcurrentDictionary<Type, Func<IInvocation, object>> _handleCaches = new ConcurrentDictionary<Type, Func<IInvocation, object>>();

        public Func<IInvocation, object> GetHandler(Type type)
        {
            return _handleCaches.GetOrAdd(type, k =>
            {
                var invocationParameterExpression = Expression.Parameter(typeof(IInvocation));
                var t = Expression.Lambda<Func<IInvocation, object>>(
                    Expression.Call(Expression.Constant(this), nameof(HandleAsync), new[] { type.GenericTypeArguments[0] },
                        invocationParameterExpression), invocationParameterExpression).Compile();
                return t;
            });
        }

        private IDictionary<string, string> GetTemplateArguments(MethodDescriptor methodDescriptor, IDictionary<string, object> arguments)
        {
            var templateVariables = new Dictionary<string, string>(methodDescriptor.Parameters.Length, StringComparer.OrdinalIgnoreCase);

            foreach (var parameterDescriptor in methodDescriptor.Parameters)
            {
                var name = parameterDescriptor.Name ?? parameterDescriptor.ParameterName;

                var goParameterAttribute = parameterDescriptor.Attributes.OfType<GoParameterAttribute>().FirstOrDefault();
                if (goParameterAttribute == null)
                    continue;
                var value = arguments[parameterDescriptor.ParameterName];
                templateVariables[name] = GetParameterExpander(goParameterAttribute.Expander).Expand(value);
            }

            return templateVariables;
        }

        private IParameterExpander GetParameterExpander(Type expanderType)
        {
            if (expanderType == null)
            {
                return null;
            }
            return _parameterExpanderLocator.Get(expanderType);
        }

        private void SetRequestLine(HttpRequestMessage request, MethodDescriptor methodDescriptor, IDictionary<string, string> templateArguments)
        {
            var requestLineTemplate = methodDescriptor.RequestLine;

            var requestLine =
                requestLineTemplate.NeedParse ?
                    _templateParser.Parse(requestLineTemplate.Template, templateArguments)
                    : requestLineTemplate.Template;

            request.RequestUri = new Uri(requestLine, UriKind.RelativeOrAbsolute);
        }

        private void SetHeaders(HttpRequestMessage request, MethodDescriptor methodDescriptor,
            IDictionary<string, string> templateArguments)
        {
            foreach (var headerItem in methodDescriptor.Headers)
            {
                var name = headerItem.Key;
                var headerTemplate = headerItem.Value;
                var value = headerTemplate.NeedParse
                    ? _templateParser.Parse(headerTemplate.Template, templateArguments)
                    : headerTemplate.Template;

                request.Headers.TryAddWithoutValidation(name, value);
            }
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            if (string.Equals(method, "Get", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Get;
            if (string.Equals(method, "Delete", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Delete;
            if (string.Equals(method, "Head", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Head;
            if (string.Equals(method, "Options", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Options;
            if (string.Equals(method, "Post", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Post;
            if (string.Equals(method, "Put", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Put;
            if (string.Equals(method, "Trace", StringComparison.OrdinalIgnoreCase)) return HttpMethod.Trace;

            return new HttpMethod(method);
        }

        private async Task<object> DoHandleAsync(IInvocation invocation)
        {
            var type = _clientDescriptor.Type;
            var method = invocation.Method;
            var returnType = method.ReturnType;
            var methodDescriptorTable = _clientDescriptor.MethodDescriptorTable;
            var methodDescriptor = methodDescriptorTable.Get(type, method);

            var response = await SendAsync(invocation, methodDescriptor);

            var isTask = typeof(Task).IsAssignableFrom(returnType);
            var realReturnType = isTask && returnType.IsGenericType ? returnType.GenericTypeArguments[0] : (isTask ? null : returnType);
            var result = await methodDescriptor.Decoder.DecodeAsync(response, realReturnType);

            return result;
        }

        private async Task<HttpResponseMessage> SendAsync(IInvocation invocation, MethodDescriptor methodDescriptor)
        {
            var arguments = invocation.Arguments;
            var method = invocation.Method;

            var request = new HttpRequestMessage
            {
                Method = GetHttpMethod(methodDescriptor.RequestMethod)
            };

            IDictionary<string, object> argumentDictionary = new Dictionary<string, object>(arguments.Length);

            for (var i = 0; i < arguments.Length; i++)
            {
                argumentDictionary[method.GetParameters()[i].Name] = arguments[i];
            }

            var templateArguments = GetTemplateArguments(methodDescriptor, argumentDictionary);
            SetRequestLine(request, methodDescriptor, templateArguments);
            SetHeaders(request, methodDescriptor, templateArguments);

            var bodyParameterDescriptor = methodDescriptor.Parameters.SingleOrDefault(i => i.Attributes.OfType<GoBodyAttribute>().Any());

            if (bodyParameterDescriptor != null)
            {
                await methodDescriptor.Encoder.EncodeAsync(argumentDictionary[bodyParameterDescriptor.Name], bodyParameterDescriptor.ParameterType, request);
            }

            return await _httpClient.SendAsync(request);
        }
    }
}