// <copyright file="ResilientCommand.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An abstract command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class ResilientCommand<TResult>
        where TResult : class
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientCommand{TResult}"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="circuitBreaker">The circuit breaker.</param>
        /// <param name="executionTimeout">The execution timeout.</param>
        /// <param name="collapser">The collapser.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="configuration">The configuration.</param>
        public ResilientCommand(
            CommandKey commandKey = null,
            CircuitBreaker circuitBreaker = null,
            ExecutionTimeout executionTimeout = null,
            Collapser collapser = null,
            ICache cache = null,
            CommandConfiguration configuration = null)
        {
            this.commandKey = commandKey ?? new CommandKey(this.GetType().Name);
            this.configuration = configuration ?? CommandConfiguration.CreateConfiguration();

            this.eventNotifier = this.InitEventNotifier();
            this.circuitBreaker = this.InitCircuitBreaker(circuitBreaker);
            this.executionTimeout = this.InitExecutionTimeout(executionTimeout);
            this.semaphore = this.InitSemaphore();
            this.collapser = this.InitCollapser(collapser);
            this.resultCache = this.InitCache(cache);
        }

        private bool IsCachedResponseEnabled => this.GetCacheKey() != null;

        /// <summary>Executes <see cref="RunAsync(CancellationToken)"/> and wraps it in the enabled features.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   A task.
        /// </returns>
        public async Task<TResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            string cacheKey = $"{this.commandKey}_{this.GetCacheKey()}";

            TResult result;
            if (this.IsCachedResponseEnabled && this.resultCache.TryGet(cacheKey, out result))
            {
                this.eventNotifier.RaiseEvent(ResilientCommandEventType.ResponseFromCache, this.commandKey);
                return result;
            }

            try
            {
                await this.semaphore.WaitAsync();

                result = await this.WrappedExecutionAsync(cancellationToken);

                if (this.IsCachedResponseEnabled)
                {
                    this.resultCache.TryAdd(cacheKey, result);
                }

                this.eventNotifier.RaiseEvent(ResilientCommandEventType.Success, this.commandKey);
                return result;
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    default:
                        this.eventNotifier.RaiseEvent(ResilientCommandEventType.Failure, this.commandKey);
                        break;
                }

                return this.HandleFallback(ex);
            }
            finally
            {
                this.semaphore.Release();
            }
        }

        /// <summary>
        /// Override this to enable fallback.
        /// </summary>
        /// <returns>A fallback result.</returns>
        protected virtual TResult Fallback()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Override this to enable caching.
        /// The cache will be per <see cref="CommandKey"/>.
        /// </summary>
        /// <returns>A cache-key.</returns>
        protected virtual string GetCacheKey()
        {
            return null;
        }

        /// <summary>
        /// The inner task to be run as part of <see cref="ExecuteAsync(CancellationToken)"/>.
        /// </summary>
        /// <remarks>
        /// In order for the CircuitBreaker to work, please re-throw any exceptions.
        /// </remarks>
        /// <param name="cancellationToken">A cancellationToken.</param>
        /// <returns>A Task.</returns>
        protected abstract Task<TResult> RunAsync(CancellationToken cancellationToken);

        private TResult HandleFallback(Exception innerException)
        {
            if (!this.configuration.FallbackSettings.IsEnabled)
            {
                this.eventNotifier.RaiseEvent(ResilientCommandEventType.FallbackDisabled, this.commandKey);
                throw innerException;
            }

            if (!this.HasFallback())
            {
                this.eventNotifier.RaiseEvent(ResilientCommandEventType.FallbackMissing, this.commandKey);
                throw innerException;
            }

            return this.Fallback();
        }

        private bool HasFallback()
        {
            if (ContainsFallback.TryGetValue(this.commandKey, out bool hasFallback))
            {
                return hasFallback;
            }

            var methodInfo = this.GetType().GetMethod(nameof(this.Fallback), BindingFlags.Instance | BindingFlags.NonPublic);
            hasFallback = methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;

            ContainsFallback.TryAdd(this.commandKey, hasFallback);

            return hasFallback;
        }

        private ICache InitCache(ICache cache)
        {
            if (this.IsCachedResponseEnabled)
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
            return SemaphoreFactory.GetOrCreateSemaphore(this.commandKey, this.configuration.MaxParallelism);
        }

        private async Task<TResult> WrappedExecutionAsync(CancellationToken cancellationToken)
        {
            Task<TResult> collapsedTask = null;
            if (this.configuration.CollapserSettings.IsEnabled)
            {
                collapsedTask = this.collapser.ExecuteAsync(this.RunAsync, cancellationToken);
            }

            Task<TResult> timeoutTask = null;
            if (this.configuration.ExecutionTimeoutSettings.IsEnabled)
            {
                timeoutTask = this.executionTimeout.ExecuteAsync(
                    innerAction: (ct) => collapsedTask ?? this.RunAsync(ct),
                    cancellationToken);
            }

            Task<TResult> circuitBreakerTask = null;
            if (this.configuration.CircuitBreakerSettings.IsEnabled)
            {
                circuitBreakerTask = this.circuitBreaker.ExecuteAsync(
                    innerAction: (ct) => timeoutTask ?? this.RunAsync(ct),
                    cancellationToken);
            }

            Task<TResult> resultTask = circuitBreakerTask ?? timeoutTask ?? collapsedTask ?? this.RunAsync(cancellationToken);

            return await resultTask;
        }
    }
}
