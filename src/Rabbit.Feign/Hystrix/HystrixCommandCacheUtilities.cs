using Steeltoe.CircuitBreaker.Hystrix;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Feign.Hystrix
{
    internal class HystrixCommandCacheUtilities
    {
        private static readonly ConcurrentDictionary<MethodInfo, HystrixCommandCacheEntry> Caches = new ConcurrentDictionary<MethodInfo, HystrixCommandCacheEntry>();

        public static HystrixCommandCacheEntry GetEntry(MethodInfo method, Func<IHystrixCommandOptions> optionsFactory)
        {
            return Caches.GetOrAdd(method, k =>
            {
                var returnType = method.ReturnType;

                if (typeof(Task).IsAssignableFrom(returnType))
                {
                    returnType = returnType.IsGenericType ? returnType.GenericTypeArguments[0] : null;
                }

                if (returnType == typeof(void))
                {
                    returnType = null;
                }

                if (returnType == null)
                {
                    returnType = typeof(int);
                }

                var commandType = typeof(SimpleHystrixCommand<>).MakeGenericType(returnType);

                var commandFactory = CreateCommandFactory(returnType);
                var methodInvoker = CreateInvoker(method);
                var executeInvoker =
                    CreateInvoker(commandType.GetMethod("ExecuteAsync", Enumerable.Empty<Type>().ToArray()));

                return new HystrixCommandCacheEntry(commandFactory, methodInvoker, executeInvoker, optionsFactory());
            });
        }

        #region Private Method

        private static HystrixCommandFactoryDelegate CreateCommandFactory(Type type)
        {
            var commandType = typeof(SimpleHystrixCommand<>).MakeGenericType(type);

            var optionsParameterExpression = Expression.Parameter(typeof(IHystrixCommandOptions), "options");
            var runParameterExpression = Expression.Parameter(typeof(Func<object>), "run");
            var fallbackParameterExpression = Expression.Parameter(typeof(Func<object>), "fallback");

            var newExpression = Expression.New(commandType.GetConstructors().First(), optionsParameterExpression, runParameterExpression, fallbackParameterExpression);

            return Expression.Lambda<HystrixCommandFactoryDelegate>(newExpression, optionsParameterExpression,
                runParameterExpression, fallbackParameterExpression).Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodBase method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) => Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).Cast<Expression>().ToArray();
        }

        private static LateBoundMethod CreateInvoker(MethodInfo method)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "target");
            var argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            Expression callInstanceExpression = instanceParameter;
            if (method.DeclaringType != null)
            {
                callInstanceExpression = Expression.Convert(instanceParameter, method.DeclaringType);
            }

            var call = Expression.Call(callInstanceExpression, method, CreateParameterExpressions(method, argumentsParameter));

            var lambda = Expression.Lambda<LateBoundMethod>(
                call,
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
        }

        #endregion Private Method

        #region Help Type

        public delegate object HystrixCommandFactoryDelegate(IHystrixCommandOptions options, Func<object> run, Func<object> fallback);

        public delegate object LateBoundMethod(object target, object[] arguments);

        public struct HystrixCommandCacheEntry
        {
            public HystrixCommandCacheEntry(HystrixCommandFactoryDelegate hystrixCommandFactory, LateBoundMethod methodInvoker, LateBoundMethod executeInvoker, IHystrixCommandOptions options)
            {
                HystrixCommandFactory = hystrixCommandFactory;
                MethodInvoker = methodInvoker;
                ExecuteInvoker = executeInvoker;
                Options = options;
            }

            public HystrixCommandFactoryDelegate HystrixCommandFactory { get; }

            public LateBoundMethod MethodInvoker { get; }
            public LateBoundMethod ExecuteInvoker { get; }
            public IHystrixCommandOptions Options { get; }
        }

        #endregion Help Type
    }
}