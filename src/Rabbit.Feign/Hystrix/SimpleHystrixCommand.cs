using Steeltoe.CircuitBreaker.Hystrix;
using System;
using System.Threading.Tasks;

namespace Rabbit.Feign.Hystrix
{
    public class SimpleHystrixCommand<T> : HystrixCommand<T>
    {
        private readonly Func<object> _genRun;

        private readonly Func<object> _genFallback;

        public SimpleHystrixCommand(IHystrixCommandOptions options, Func<object> run, Func<object> fallback) : base(options)
        {
            _genRun = run;
            _genFallback = fallback;
        }

        #region Overrides of HystrixCommand<T>

        /// <inheritdoc/>
        protected override T Run()
        {
            return Task.Run(async () => await RunAsync()).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        protected override Task<T> RunAsync()
        {
            var result = _genRun();

            if (result is Task<T> task)
            {
                return task;
            }

            if (result is T value)
            {
                return Task.FromResult(value);
            }

            return Task.FromResult(default(T));
        }

        protected override Task<T> RunFallbackAsync()
        {
            if (_genFallback == null)
            {
                return base.RunFallbackAsync();
            }
            var result = _genFallback();

            if (result is Task<T> task)
            {
                return task;
            }

            if (result is T value)
            {
                return Task.FromResult(value);
            }

            return Task.FromResult(default(T));
        }

        #endregion Overrides of HystrixCommand<T>
    }
}