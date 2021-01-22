using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public abstract class ResilientCommand<TResult> where TResult : class
    {
        private readonly ConcurrentDictionary<string, TResult> resultCache = new ConcurrentDictionary<string, TResult>();
        private readonly CircuitBreaker circuitBreaker;
        private readonly ExecutionTimeout executionTimeout;
        private readonly SemaphoreSlim semaphore;
        private readonly CommandKey commandKey;
        private readonly CommandConfiguration configuration;
        private bool IsCachedResponseEnabled => GetCacheKey() != null;

        public ResilientCommand(CommandKey commandKey = null, CommandConfiguration configuration = null)
        {
            this.commandKey = commandKey ?? new CommandKey(GetType().Name);

            this.configuration = configuration ?? CommandConfiguration.CreateConfiguration();

            circuitBreaker = initCircuitBreaker();
            executionTimeout = initExecutionTimeout();
            semaphore = initSemaphore();
            

        }

        private SemaphoreSlim initSemaphore()
        {
            return SemaphoreFactory.GetOrCreateSemaphore(commandKey, this.configuration.MaxParallelism);
        }

        private ExecutionTimeout initExecutionTimeout()
        {
            if (this.configuration.ExecutionTimeoutSettings.IsEnabled)
            {
                return new ExecutionTimeout(this.configuration.ExecutionTimeoutSettings);
            }

            return null;
        }

        private CircuitBreaker initCircuitBreaker()
        {
            if (this.configuration.CircuitBreakerSettings.IsEnabled)
            {
                return CircuitBreakerFactory.Instance.GetOrCreateCircuitBreaker(commandKey, this.configuration.CircuitBreakerSettings);  
            }

            return null;
        }

        public abstract Task<TResult> RunAsync(CancellationToken cancellationToken);

        public virtual TResult Fallback()
        {
            return null;
        }


        public virtual string GetCacheKey()
        {
            return null;
        }

        public async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            string cacheKey = $"{commandKey}_{GetCacheKey()}";

            TResult result;
            if (IsCachedResponseEnabled && resultCache.TryGetValue(cacheKey, out result))
            {
                return result;
            }

            try
            {
                await semaphore.WaitAsync();
                
                result = await WrappedExecution(cancellationToken);

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
                    return fallbackValue;
                }

                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<TResult> WrappedExecution(CancellationToken cancellationToken)
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
