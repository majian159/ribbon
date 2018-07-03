using Castle.DynamicProxy;
using Rabbit.Feign.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
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
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                if (returnType.IsGenericType)
                {
                    var method = typeof(FeignInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(invocation.Method.ReturnType.GenericTypeArguments[0]);
                    invocation.ReturnValue = method.Invoke(this, new object[] { invocation });
                }
                else
                {
                    invocation.ReturnValue = HandleTaskAsync(invocation);
                }
            }
            else
            {
                invocation.ReturnValue = Handle(invocation);
            }
        }

        #endregion Implementation of IInterceptor

        private async Task HandleTaskAsync(IInvocation invocation)
        {
            await DoHandleAsync(invocation);
        }

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var value = await DoHandleAsync(invocation);

            switch (value)
            {
                case null:
                    return default(T);

                case Task<T> task:
                    return await task;
            }

            return (T)value;
        }

        private object Handle(IInvocation invocation)
        {
            return DoHandleAsync(invocation).GetAwaiter().GetResult();
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

            request.RequestUri = new Uri(requestLine,UriKind.RelativeOrAbsolute);
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

        private async Task<object> DoHandleAsync(IInvocation invocation)
        {
            var type = _clientDescriptor.Type;
            var arguments = invocation.Arguments;
            var method = invocation.Method;
            var returnType = method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);

            var realReturnType = isTask && returnType.IsGenericType ? returnType.GenericTypeArguments[0] : (isTask ? null : returnType);

            var methodDescriptorTable = _clientDescriptor.MethodDescriptorTable;

            var methodDescriptor = methodDescriptorTable.Get(type, method);

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(methodDescriptor.RequestMethod)
            };

            IDictionary<string, object> argumentDictionary = new Dictionary<string, object>();

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
                await methodDescriptor.Encoder.EncodeAsync(argumentDictionary[bodyParameterDescriptor.Name], request);
            }

            var resposne = await _httpClient.SendAsync(request);

            var result = await methodDescriptor.Decoder.DecodeAsync(resposne,realReturnType);

            return result;
        }
    }
}