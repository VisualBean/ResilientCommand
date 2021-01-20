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
        private readonly Timeout timeout;
        private readonly SemaphoreSlim semaphore;
        private readonly CommandKey commandKey;

        private bool IsCachedResponseEnabled => GetCacheKey() != null;

        public ResilientCommand(CommandKey commandKey = null, int timeoutInMiliseconds = 10000, CircuitBreakerSettings circuitBreakerSettings = null, int maxParallelism = 10)
        {
            if (commandKey == null)
            {
                this.commandKey = new CommandKey(GetType().Name);
            }

            circuitBreaker = CircuitBreakerFactory.GetOrCreateCircuitBreaker(commandKey, circuitBreakerSettings);
            semaphore = SemaphoreFactory.GetOrCreateSemaphore(commandKey, maxParallelism);
            timeout = new Timeout(timeoutInMiliseconds);
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
                //TODO: Potential failure if cancellationToken is already cancelled, then we release and throw due to maxsemaphore.
                await semaphore.WaitAsync(cancellationToken);

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
