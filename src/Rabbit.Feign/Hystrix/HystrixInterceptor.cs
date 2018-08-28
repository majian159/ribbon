using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CircuitBreaker.Hystrix.Strategy;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rabbit.Feign.Hystrix
{
    public class HystrixInterceptor : IInterceptor
    {
        private readonly string _clientName;
        private readonly Type _pryxyType;
        private readonly object _proxyInstance;
        private readonly Type _fallbackType;
        private readonly IServiceProvider _services;

        private readonly Func<object> _fallbackInstanceFactory;

        public HystrixInterceptor(string clientName, Type pryxyType, object proxyInstance, Type fallbackType, IServiceProvider services)
        {
            _clientName = clientName;
            _pryxyType = pryxyType;
            _proxyInstance = proxyInstance;
            _fallbackType = fallbackType;

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

            var cache = HystrixCommandCacheUtilities.GetEntry(method, _fallbackType, () => CreateCommandOptions(method));

            var arguments = invocation.Arguments;

            object RunFunc() => cache.MethodInvoker(_proxyInstance, arguments);
            Func<object> fallbackFunc = null;

            if (_fallbackInstanceFactory != null && cache.FallbackMethodInvoker != null)
            {
                fallbackFunc = () => cache.FallbackMethodInvoker(_fallbackInstanceFactory(), arguments);
            }

            var command = cache.HystrixCommandFactory(cache.Options, RunFunc, fallbackFunc);

            return cache.ExecuteInvoker(command, null);
        }

        private IHystrixCommandOptions CreateCommandOptions(MethodBase method)
        {
            var groupKeyName = _clientName;
            var commandKeyName = GetCommandKey(_pryxyType, method);

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

        private static string GetCommandKey(MemberInfo targetType, MethodBase method)
        {
            var builder = new StringBuilder();
            builder.Append(targetType.Name).Append("#").Append(method.Name).Append("(");

            var parameters = method.GetParameters();

            foreach (var parameterInfo in parameters)
            {
                builder.Append(parameterInfo.ParameterType.Name).Append(",");
            }

            if (parameters.Any())
            {
                builder.Remove(builder.Length - 1, 1);
            }

            return builder.Append(")").ToString();
        }
    }
}