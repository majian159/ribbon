using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Rabbit.Feign.Codec;
using Rabbit.Feign.Hystrix;
using Rabbit.Feign.Reflective;
using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rabbit.Feign
{
    public class FeignBuilder
    {
        private readonly IServiceProvider _services;
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private IEncoder _encoder = DefaultEncoder.Default;
        private IDecoder _decoder = DefaultDecoder.Default;
        private string _clientName;
        private Type _fallbackType;

        public FeignBuilder(IServiceProvider services)
        {
            _services = services;
        }

        public FeignBuilder Encoder(IEncoder encoder)
        {
            _encoder = encoder;
            return this;
        }

        public FeignBuilder Decoder(IDecoder decoder)
        {
            _decoder = decoder;
            return this;
        }

        public FeignBuilder ClientName(string clientName)
        {
            _clientName = clientName;
            return this;
        }

        public FeignBuilder FallbackType(Type fallbackType)
        {
            _fallbackType = fallbackType;
            return this;
        }

        // private GoInterceptor _goInterceptor;

        public object Target(Type type)
        {
            return Target(type, null);
        }

        public object Target(Type type, string url)
        {
            var clientDescriptor = new ClientDescriptor
            {
                MethodDescriptorTable = new MethodDescriptorTable(),
                Name = _clientName,
                Type = type
            };
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                clientDescriptor.MethodDescriptorTable.Set(type, method);
                var methodDescriptor = clientDescriptor.MethodDescriptorTable.Get(type, method);
                if (methodDescriptor.Decoder == null)
                {
                    methodDescriptor.Decoder = _decoder;
                }

                if (methodDescriptor.Encoder == null)
                {
                    methodDescriptor.Encoder = _encoder;
                }
            }

            var httpClientFactory = _services.GetRequiredService<IHttpClientFactory>();
            var parameterExpanderLocator = _services.GetRequiredService<IParameterExpanderLocator>();

            var goInterceptor = new FeignInterceptor(clientDescriptor, httpClientFactory, parameterExpanderLocator);

            var feignProxy = ProxyGenerator.CreateInterfaceProxyWithoutTarget(type, Enumerable.Empty<Type>().ToArray(), goInterceptor);

            return ProxyGenerator.CreateInterfaceProxyWithTarget(type, feignProxy,
                new HystrixInterceptor(type, feignProxy, _fallbackType, _services));
        }

        public T Target<T>()
        {
            return Target<T>(null);
        }

        public T Target<T>(string url)
        {
            return (T)Target(typeof(T), url);
        }
    }

    public static class FeignBuilderExtensions
    {
        public static T TargetByAttribute<T>(this FeignBuilder builder)
        {
            return (T)builder.TargetByAttribute(typeof(T));
        }

        public static object TargetByAttribute(this FeignBuilder builder, Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var feignClientAttribute = type.GetCustomAttribute<FeignClientAttribute>();

            if (feignClientAttribute == null)
            {
                throw new ArgumentException("can't find FeignClient Attribute.");
            }

            return builder
                .ClientName(feignClientAttribute.Name)
                .FallbackType(feignClientAttribute.FallbackType)
                .Target(type);
        }
    }
}