/*using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CircuitBreaker.Hystrix.Strategy;

namespace Rabbit.Feign.Hystrix.Options
{
    public class HystrixCommandFactoryOptions
    {
        public Type Type { get; set; }
        public MethodInfo Method { get; set; }

        public Func<object, object[], object> HystrixCommandFactory { get; set; }

        public void Init(Type type, MethodInfo method)
        {
            Type = type;
            Method = method;
        }
    }

    public class HystrixCommandFactoryOptionsSetup : IConfigureNamedOptions<HystrixCommandFactoryOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _services;

        public HystrixCommandFactoryOptionsSetup(IConfiguration configuration, IServiceProvider services)
        {
            _configuration = configuration;
            _services = services;
        }

        #region Implementation of IConfigureOptions<in HystrixCommandFactoryOptions>

        /// <inheritdoc/>
        public void Configure(HystrixCommandFactoryOptions options)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of IConfigureOptions<in HystrixCommandFactoryOptions>

        #region Implementation of IConfigureNamedOptions<in HystrixCommandFactoryOptions>

        /// <inheritdoc/>
        public void Configure(string name, HystrixCommandFactoryOptions options)
        {
            var method = options.Method;
            var type = options.Type;

            var hystrixCommandAttribute = method.GetCustomAttribute<HystrixCommandAttribute>();

            var groupKeyName = hystrixCommandAttribute.GroupKey;
            var commandKeyName = hystrixCommandAttribute.CommandKey;
            var threadPoolKeyName = hystrixCommandAttribute.ThreadPoolKey;
            var fallbackMethodName = hystrixCommandAttribute.FallbackMethod;

            if (string.IsNullOrEmpty(groupKeyName))
            {
                groupKeyName = type.Name;
            }

            if (string.IsNullOrEmpty(commandKeyName))
            {
                commandKeyName = method.Name;
            }

            if (string.IsNullOrEmpty(fallbackMethodName))
            {
                fallbackMethodName = method.Name + "Fallback";
            }

            var groupKey = HystrixCommandGroupKeyDefault.AsKey(groupKeyName);
            var commandKey = HystrixCommandKeyDefault.AsKey(commandKeyName);
            var threadPoolKey = string.IsNullOrWhiteSpace(threadPoolKeyName) ? null : HystrixThreadPoolKeyDefault.AsKey(threadPoolKeyName);

            var strategy = HystrixPlugins.OptionsStrategy;
            var dynOpts = strategy.GetDynamicOptions(_configuration);

            var opts = new HystrixCommandOptions(commandKey, null, dynOpts)
            {
                GroupKey = groupKey,
                ThreadPoolKey = threadPoolKey
            };

            Console.WriteLine("CircuitBreakerForceOpen:" + opts.CircuitBreakerEnabled);

            var fallbackMethod = type.GetMethod(fallbackMethodName, method.GetParameters().Select(p => p.ParameterType).ToArray());

            if (fallbackMethod == null)
            {
                return;
            }

            options.HystrixCommandFactory = (instance, args) =>
            {
                var returnType = method.ReturnType;

                var isTask = typeof(Task).IsAssignableFrom(returnType);

                var realType = isTask ? returnType.IsGenericType ? returnType.GenericTypeArguments[0] : typeof(object) : returnType;

                var instancExpression = Expression.Constant(instance);
                var parameterExpressions = method.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).Cast<Expression>().ToArray();

                var mainCallExpression = Expression.Call(instancExpression, method, parameterExpressions);
                var fallbackCallExpression = Expression.Call(instancExpression, fallbackMethod, parameterExpressions);

                var mainDelegate = Expression.Lambda(mainCallExpression).Compile();
                var fallbackDelegate = Expression.Lambda(fallbackCallExpression).Compile();

                var commandType = typeof(SimpleHystrixCommand<>).MakeGenericType(realType);

                var loggerFactory = _services.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger(commandType);

                var constructorInfo = commandType.GetConstructor(new[] { typeof(IHystrixCommandOptions), mainDelegate.GetType(), fallbackDelegate.GetType(), typeof(ILogger) });
                return constructorInfo.Invoke(new object[] { opts, mainDelegate, fallbackDelegate, logger });
            };
        }

        #endregion Implementation of IConfigureNamedOptions<in HystrixCommandFactoryOptions>
    }
}*/