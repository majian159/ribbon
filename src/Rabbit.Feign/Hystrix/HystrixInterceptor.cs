using Castle.DynamicProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CircuitBreaker.Hystrix.Strategy;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Feign.Hystrix
{
    public class HystrixInterceptor : IInterceptor
    {
        private readonly Type _pryxyType;
        private readonly object _proxyInstance;
        private readonly Type _hystrixType;
        private readonly IServiceProvider _services;
        private ConcurrentDictionary<MethodInfo, Func<object[], object>> _commands = new ConcurrentDictionary<MethodInfo, Func<object[], object>>();

        public HystrixInterceptor(Type pryxyType, object proxyInstance, Type hystrixType, IServiceProvider services)
        {
            _pryxyType = pryxyType;
            _proxyInstance = proxyInstance;
            _hystrixType = hystrixType;
            _services = services;
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
                    var method = typeof(HystrixInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance)
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

        private async Task<object> DoHandleAsync(IInvocation invocation)
        {
            var method = _pryxyType.GetMethod(invocation.Method.Name,
                invocation.Method.GetParameters().Select(i => i.ParameterType).ToArray());
            var hystrixCommandFactory = _commands.GetOrAdd(method, key => CreateHystrixCommand(key, invocation));
            var hystrixCommand = hystrixCommandFactory(invocation.Arguments);

            var returnType = method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);

            MethodCallExpression callExpression;
            if (isTask)
            {
                callExpression = Expression.Call(Expression.Constant(hystrixCommand), "ExecuteAsync", null);
            }
            else
            {
                callExpression = Expression.Call(Expression.Constant(hystrixCommand), "Execute", null);
            }

            var invoker = Expression.Lambda<Func<object>>(callExpression).Compile();

            var result = invoker();
            if (result is Task task)
            {
                await task;
            }

            return result;
        }

        private Func<object[], object> CreateHystrixCommand(MethodInfo method, IInvocation invocation)
        {
            var groupKeyName = _pryxyType.Name;
            var commandKeyName = method.Name;
            var fallbackMethodName = method.Name;

            var groupKey = HystrixCommandGroupKeyDefault.AsKey(groupKeyName);
            var commandKey = HystrixCommandKeyDefault.AsKey(commandKeyName);

            var configuration = _services.GetService<IConfiguration>();
            var strategy = HystrixPlugins.OptionsStrategy;
            var dynOpts = strategy.GetDynamicOptions(configuration);
            var opts = new HystrixCommandOptions(commandKey, null, dynOpts)
            {
                GroupKey = groupKey
            };

            var fallbackMethod = _hystrixType.GetMethod(fallbackMethodName, method.GetParameters().Select(p => p.ParameterType).ToArray());

            if (fallbackMethod == null)
            {
                return null;
            }

            return (args) =>
            {
                var returnType = method.ReturnType;

                var isTask = typeof(Task).IsAssignableFrom(returnType);

                var realType = isTask ? returnType.IsGenericType ? returnType.GenericTypeArguments[0] : typeof(object) : returnType;

                var fallbackInstance = ActivatorUtilities.GetServiceOrCreateInstance(_services, _hystrixType);

                var parameters = method.GetParameters();
                var argumentsExpressions = args.Select((a, index) =>
                {
                    var p = parameters[index];

                    Expression expression = Expression.Constant(a);

                    if (a.GetType() != p.ParameterType)
                    {
                        expression = Expression.Convert(expression, p.ParameterType);
                    }

                    return expression;
                }).ToArray();

                object GetDelegate(object instance, MethodInfo delegateMethod)
                {
                    var instancExpression = Expression.Constant(instance);
                    var parameterExpressions = delegateMethod.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
                    var callExpression = Expression.Call(instancExpression, method, parameterExpressions);

                    return Expression.Lambda(Expression.Invoke(Expression.Lambda(callExpression, parameterExpressions), argumentsExpressions)).Compile();
                }

                var mainDelegate = GetDelegate(_proxyInstance, method);
                var fallbackDelegate = GetDelegate(fallbackInstance, fallbackMethod);

                var commandType = typeof(SimpleHystrixCommand<>).MakeGenericType(realType);
                var loggerFactory = _services.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger(commandType);

                var constructorInfo = commandType.GetConstructor(new[] { typeof(IHystrixCommandOptions), mainDelegate.GetType(), fallbackDelegate.GetType(), typeof(ILogger) });
                return constructorInfo.Invoke(new object[] { opts, mainDelegate, fallbackDelegate, logger });
            };
        }
    }
}