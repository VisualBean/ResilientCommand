// <copyright file="CircuitBreaker.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Polly;
    using Polly.CircuitBreaker;

    /// <summary>
    /// A rolling-window circuit-breaker implementation.
    /// </summary>
    /// <seealso cref="ExecutionDecorator" />
    public class CircuitBreaker : ExecutionDecorator
    {
        /// <summary>
        /// The circuit-breaker policy.
        /// </summary>
        private readonly AsyncCircuitBreakerPolicy circuitbreakerPolicy;

        /// <summary>
        /// The command key.
        /// </summary>
        private readonly CommandKey commandKey;

        /// <summary>
        /// The settings.
        /// </summary>
        private readonly CircuitBreakerSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreaker"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="settings">The settings.</param>
        public CircuitBreaker(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CircuitBreakerSettings settings = null)
        {
            this.settings = settings ?? CircuitBreakerSettings.DefaultCircuitBreakerSettings;

            this.circuitbreakerPolicy = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: this.settings.FailureThreshhold,
                samplingDuration: TimeSpan.FromMilliseconds(this.settings.SamplingDurationMilliseconds),
                minimumThroughput: this.settings.MinimumThroughput,
                durationOfBreak: TimeSpan.FromMilliseconds(this.settings.DurationMilliseconds),
                onBreak: (ex, ts) =>
                {
                    eventNotifier.RaiseEvent(ResilientCommandEventType.CircuitBroken, commandKey);
                    throw new CircuitBrokenException(this.commandKey, ex);
                },
                onReset: () => eventNotifier.RaiseEvent(ResilientCommandEventType.CircuitReset, commandKey));

            this.commandKey = commandKey;
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="innerAction">The inner action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task.</returns>
        /// <exception cref="CircuitBrokenException">When the circuit is in an open state, it will throw.</exception>
        public override async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!this.settings.IsEnabled)
            {
                return await innerAction(cancellationToken);
            }

            if (this.circuitbreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new CircuitBrokenException(this.commandKey);
            }

            return await this.circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
