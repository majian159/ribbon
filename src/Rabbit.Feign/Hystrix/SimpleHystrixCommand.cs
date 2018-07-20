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
        private readonly ILogger _logger;

        public SimpleHystrixCommand(IHystrixCommandOptions options, Func<T> run, Func<T> fallback, ILogger logger = null) : base(options, run, fallback, logger)
        {
        }

        public SimpleHystrixCommand(IHystrixCommandOptions options, Func<Task<T>> runAsync, Func<Task<T>> fallbackAsync, ILogger logger = null) : base(options, null, null, logger)
        {
            _runAsync = runAsync;
            _fallbackAsync = fallbackAsync;
            _logger = logger;
        }

        #region Overrides of HystrixCommand<T>

        /// <inheritdoc/>
        protected override T Run()
        {
            return Task.Run(async () => await RunAsync()).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        protected override async Task<T> RunAsync()
        {
            try
            {
                return await _runAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HystrixCommand run execute fail.");
                throw;
            }
        }

        protected override Task<T> RunFallbackAsync()
        {
            return _fallbackAsync == null ? base.RunFallbackAsync() : _fallbackAsync();
        }

        #endregion Overrides of HystrixCommand<T>
    }
}