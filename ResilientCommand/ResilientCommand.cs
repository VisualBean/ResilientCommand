using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public abstract class ResilientCommand<TResult> where TResult : class
    {
        private readonly CircuitBreaker circuitBreaker;
        private readonly CommandKey commandKey;
        private readonly CommandConfiguration configuration;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly ExecutionTimeout executionTimeout;
        private readonly ConcurrentDictionary<string, TResult> resultCache = new ConcurrentDictionary<string, TResult>();
        private readonly SemaphoreSlim semaphore;

        public ResilientCommand(CommandKey commandKey = null, CommandConfiguration configuration = null)
        {
            this.commandKey = commandKey ?? new CommandKey(GetType().Name);
            this.configuration = configuration ?? CommandConfiguration.CreateConfiguration();

            eventNotifier = InitEventNotifier();
            circuitBreaker = InitCircuitBreaker();
            executionTimeout = InitExecutionTimeout();
            semaphore = InitSemaphore();

        }

        private bool IsCachedResponseEnabled => GetCacheKey() != null;

        public async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            string cacheKey = $"{commandKey}_{GetCacheKey()}";

            TResult result;
            if (IsCachedResponseEnabled && resultCache.TryGetValue(cacheKey, out result))
            {
                this.eventNotifier.markEvent(ResillientCommandEventType.CachedResponseUsed, this.commandKey);
                return result;
            }

            try
            {
                await semaphore.WaitAsync();

                result = await WrappedExecutionAsync(cancellationToken);

                if (IsCachedResponseEnabled)
                {
                    resultCache.TryAdd(cacheKey, result);
                }

                return result;
            }
            catch (Exception)
            {
                var fallbackValue = Fallback();
                if (fallbackValue != null)
                {
                    this.eventNotifier.markEvent(ResillientCommandEventType.FallbackUsed, this.commandKey);
                    return fallbackValue;
                }

                this.eventNotifier.markEvent(ResillientCommandEventType.FallbackSkipped, this.commandKey);
                throw;
            }
            finally
            {

                semaphore.Release();
            }
        }

        /// <summary>
        /// Override this to enable fallback.
        /// </summary>
        /// <returns></returns>
        protected virtual TResult Fallback()
        {
            return null;
        }

        /// <summary>
        /// Override this to enable caching.
        /// The cache will be per <see cref="CommandKey"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCacheKey()
        {
            return null;
        }

        /// <summary>
        /// Implementation of runAsync. When calling ExecuteAsync, runasync will be called, wrapped in 
        /// </summary>
        /// <remarks>
        /// In order for the CircuitBreaker to work, please re-throw any exceptions
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<TResult> RunAsync(CancellationToken cancellationToken);

        private CircuitBreaker InitCircuitBreaker()
        {
            if (this.configuration.CircuitBreakerSettings.IsEnabled)
            {
                return CircuitBreakerFactory.GetInstance().GetOrCreateCircuitBreaker(commandKey, this.eventNotifier, this.configuration.CircuitBreakerSettings);
            }

            return null;
        }

        private ResilientCommandEventNotifier InitEventNotifier()
        {
            return EventNotifierFactory.GetInstance().GetEventNotifier();
        }

        private ExecutionTimeout InitExecutionTimeout()
        {
            if (this.configuration.ExecutionTimeoutSettings.IsEnabled)
            {
                return new ExecutionTimeout(this.commandKey, this.eventNotifier, this.configuration.ExecutionTimeoutSettings);
            }

            return null;
        }

        private SemaphoreSlim InitSemaphore()
        {
            return SemaphoreFactory.GetOrCreateSemaphore(commandKey, this.configuration.MaxParallelism);
        }

        private async Task<TResult> WrappedExecutionAsync(CancellationToken cancellationToken)
        {
            Task<TResult> timeoutTask = null;
            if (this.configuration.ExecutionTimeoutSettings.IsEnabled)
            {
                timeoutTask = executionTimeout.ExecuteAsync(RunAsync, cancellationToken);
            }

            Task<TResult> circuitBreakerTask = null;
            if (this.configuration.CircuitBreakerSettings.IsEnabled)
            {
                circuitBreakerTask = circuitBreaker.ExecuteAsync((ct) => timeoutTask ?? RunAsync(ct), cancellationToken);
            }

            Task<TResult> resultTask = circuitBreakerTask ?? RunAsync(cancellationToken);

            return await resultTask;
        }
    }
}
