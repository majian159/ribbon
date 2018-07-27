using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CircuitBreaker.Hystrix.Strategy;
using System;
using System.Reflection;

namespace Rabbit.Feign.Hystrix
{
    public class HystrixInterceptor : IInterceptor
    {
        private readonly Type _pryxyType;
        private readonly object _proxyInstance;
        private readonly IServiceProvider _services;

        private readonly Func<object> _fallbackInstanceFactory;

        public HystrixInterceptor(Type pryxyType, object proxyInstance, Type fallbackType, IServiceProvider services)
        {
            _pryxyType = pryxyType;
            _proxyInstance = proxyInstance;

            if (fallbackType != null)
            {
                _fallbackInstanceFactory = () => ActivatorUtilities.GetServiceOrCreateInstance(services, fallbackType);
            }

            _services = services;
        }

        #region Implementation of IInterceptor

        /// <inheritdoc/>
        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = Handle(invocation);
        }

        #endregion Implementation of IInterceptor

        private object Handle(IInvocation invocation)
        {
            var method = invocation.Method;

            var cache = HystrixCommandCacheUtilities.GetEntry(method, () => CreateCommandOptions(method));

            var arguments = invocation.Arguments;

            object RunFunc() => cache.MethodInvoker(_proxyInstance, arguments);
            Func<object> fallbackFunc = null;

            if (_fallbackInstanceFactory != null)
            {
                fallbackFunc = () => cache.MethodInvoker(_fallbackInstanceFactory(), arguments);
            }

            var command = cache.HystrixCommandFactory(cache.Options, RunFunc, fallbackFunc);

            return cache.ExecuteInvoker(command, null);
        }

        private IHystrixCommandOptions CreateCommandOptions(MemberInfo method)
        {
            var groupKeyName = _pryxyType.Name;
            var commandKeyName = method.Name;

            var groupKey = HystrixCommandGroupKeyDefault.AsKey(groupKeyName);
            var commandKey = HystrixCommandKeyDefault.AsKey(commandKeyName);

            var configuration = _services.GetService<IConfiguration>();
            var strategy = HystrixPlugins.OptionsStrategy;
            var dynOpts = strategy.GetDynamicOptions(configuration);
            var opts = new HystrixCommandOptions(commandKey, null, dynOpts)
            {
                GroupKey = groupKey
            };

            return opts;
        }
    }
}