using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;
using System;
using System.Threading.Tasks;

namespace Rabbit.Feign.Hystrix
{
    public class SimpleHystrixCommand<T> : HystrixCommand<T>
    {
        private readonly Func<Task<T>> _runAsync;
        private readonly Func<Task<T>> _fallbackAsync;

        public SimpleHystrixCommand(IHystrixCommandOptions options, Func<T> run, Func<T> fallback, ILogger logger = null) : base(options, run, fallback, logger)
        {
        }

        public SimpleHystrixCommand(IHystrixCommandOptions options, Func<Task<T>> runAsync, Func<Task<T>> fallbackAsync, ILogger logger = null) : base(options, null, null, logger)
        {
            _runAsync = runAsync;
            _fallbackAsync = fallbackAsync;
        }

        #region Overrides of HystrixCommand<T>

        /// <inheritdoc/>
        protected override Task<T> RunAsync()
        {
            return _runAsync();
        }

        protected override Task<T> RunFallbackAsync()
        {
            return _fallbackAsync();
        }

        #endregion Overrides of HystrixCommand<T>
    }
}