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

            circuitBreaker = CircuitBreakerFactory.Instance.GetOrCreateCircuitBreaker(commandKey, this.configuration.CircuitBreakerSettings);
            executionTimeout = new ExecutionTimeout(this.configuration.ExecutionTimeoutSettings);
            semaphore = SemaphoreFactory.GetOrCreateSemaphore(commandKey, this.configuration.MaxParallelism);

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
            
            if (IsCachedResponseEnabled && resultCache.TryGetValue(cacheKey, out TResult result))
            {
                return result;
            }

            try
            {                
                await semaphore.WaitAsync();

                var task = timeout.ExecuteAsync(RunAsync, cancellationToken);
                result = await circuitBreaker.ExecuteAsync((ct) => task, cancellationToken);

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
    }
}
