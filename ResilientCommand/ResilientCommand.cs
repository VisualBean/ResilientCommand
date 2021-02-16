using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public abstract class ResilientCommand<TResult> where TResult : class
    {
        private static readonly ConcurrentDictionary<CommandKey, bool> ContainsFallback = new ConcurrentDictionary<CommandKey, bool>();
        private readonly CircuitBreaker circuitBreaker;
        private readonly Collapser collapser;
        private readonly CommandKey commandKey;
        private readonly CommandConfiguration configuration;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly ExecutionTimeout executionTimeout;
        private readonly ICache resultCache;
        private readonly SemaphoreSlim semaphore;

        public ResilientCommand(
            CommandKey commandKey = null, 
            CircuitBreaker circuitBreaker = null, 
            ExecutionTimeout executionTimeout = null, 
            Collapser collapser = null, 
            ICache cache = null, 
            CommandConfiguration configuration = null)
        {
            this.commandKey = commandKey ?? new CommandKey(GetType().Name);
            this.configuration = configuration ?? CommandConfiguration.CreateConfiguration();

            eventNotifier = InitEventNotifier();
            this.circuitBreaker = InitCircuitBreaker(circuitBreaker);
            this.executionTimeout = InitExecutionTimeout(executionTimeout);
            semaphore = InitSemaphore();
            this.collapser = InitCollapser(collapser);
            this.resultCache = InitCache(cache);
        }

        private bool IsCachedResponseEnabled => GetCacheKey() != null;

        public async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            string cacheKey = $"{commandKey}_{GetCacheKey()}";

            TResult result;
            if (IsCachedResponseEnabled && resultCache.TryGet(cacheKey, out result))
            {
                this.eventNotifier.MarkEvent(ResillientCommandEventType.ResponseFromCache, this.commandKey);
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

                this.eventNotifier.MarkEvent(ResillientCommandEventType.Success, commandKey);
                return result;
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    default:
                        this.eventNotifier.MarkEvent(ResillientCommandEventType.Failure, this.commandKey);
                        break;
                }

                return HandleFallback(ex);
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
            throw new NotImplementedException();
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
        /// Task to run as a ResilientCommand.
        /// </summary>
        /// <remarks>
        /// In order for the CircuitBreaker to work, please re-throw any exceptions
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task<TResult> RunAsync(CancellationToken cancellationToken);

        private TResult HandleFallback(Exception innerException)
        {
            if (!this.configuration.FallbackSettings.IsEnabled)
            {
                this.eventNotifier.MarkEvent(ResillientCommandEventType.FallbackDisabled, this.commandKey);
                throw innerException;
            }

            if (!HasFallback())
            {
                this.eventNotifier.MarkEvent(ResillientCommandEventType.FallbackMissing, this.commandKey);
                throw innerException;
            }

            return Fallback();
        }

        private bool HasFallback()
        {
            if (ContainsFallback.TryGetValue(this.commandKey, out bool hasFallback))
            {
                return hasFallback;
            }

            var methodInfo = GetType().GetMethod(nameof(Fallback), BindingFlags.Instance | BindingFlags.NonPublic);
            hasFallback = methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;

            ContainsFallback.TryAdd(this.commandKey, hasFallback);

            return hasFallback;
        }

        private ICache InitCache(ICache cache)
        {   
            if (IsCachedResponseEnabled)
            {
                return cache ?? new InMemoryCache();
            }

            return null;
        }

        private CircuitBreaker InitCircuitBreaker(CircuitBreaker circuitBreaker)
        {
            if (this.configuration.CircuitBreakerSettings.IsEnabled)
            {
                return circuitBreaker ?? CircuitBreakerFactory.GetInstance().GetOrCreateCircuitBreaker(this.commandKey, this.eventNotifier, this.configuration.CircuitBreakerSettings);
            }

            return null;
        }

        private Collapser InitCollapser(Collapser collapser)
        {
            if (this.configuration.CollapserSettings.IsEnabled)
            {
                return collapser ?? CollapserFactory.GetInstance().GetOrCreateCollapser(this.commandKey, this.eventNotifier, this.configuration.CollapserSettings);
            }

            return null;
        }

        private ResilientCommandEventNotifier InitEventNotifier()
        {
            return EventNotifierFactory.GetInstance().GetEventNotifier();
        }

        private ExecutionTimeout InitExecutionTimeout(ExecutionTimeout executionTimeout)
        {
            if (this.configuration.ExecutionTimeoutSettings.IsEnabled)
            {
                return executionTimeout ?? new ExecutionTimeout(this.commandKey, this.eventNotifier, this.configuration.ExecutionTimeoutSettings);
            }

            return null;
        }

        private SemaphoreSlim InitSemaphore()
        {
            return SemaphoreFactory.GetOrCreateSemaphore(commandKey, this.configuration.MaxParallelism);
        }
        private async Task<TResult> WrappedExecutionAsync(CancellationToken cancellationToken)
        {
            Task<TResult> collapsedTask = null;
            if (this.configuration.CollapserSettings.IsEnabled)
            {
                collapsedTask = collapser.ExecuteAsync(RunAsync, cancellationToken);
            }

            Task<TResult> timeoutTask = null;
            if (this.configuration.ExecutionTimeoutSettings.IsEnabled)
            {
                timeoutTask = executionTimeout.ExecuteAsync(
                    innerAction: (ct) => collapsedTask ?? RunAsync(ct), 
                    cancellationToken);
            }

            Task<TResult> circuitBreakerTask = null;
            if (this.configuration.CircuitBreakerSettings.IsEnabled)
            {
                circuitBreakerTask = circuitBreaker.ExecuteAsync(
                    innerAction: (ct) => timeoutTask ?? RunAsync(ct), 
                    cancellationToken);
            }

            Task<TResult> resultTask = circuitBreakerTask ?? timeoutTask ?? collapsedTask ?? RunAsync(cancellationToken);

            return await resultTask;
        }
    }
}
