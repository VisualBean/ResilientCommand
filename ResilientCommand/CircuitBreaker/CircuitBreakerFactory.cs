// <copyright file="CircuitBreakerFactory.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// A factory for circuit-breakers.
    /// </summary>
    internal sealed class CircuitBreakerFactory
    {
        private static readonly Lazy<CircuitBreakerFactory>
            Instance =
            new Lazy<CircuitBreakerFactory>(
                () => new CircuitBreakerFactory());

        private readonly ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>> circuitBreakerByGroup = new ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>>();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns>A <see cref="CircuitBreakerFactory"/> instance.</returns>
        internal static CircuitBreakerFactory GetInstance() => Instance.Value;

        /// <summary>
        /// Gets or creates a circuit breaker.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="circuitBreakerSettings">The circuit breaker settings.</param>
        /// <returns>A <see cref="CircuitBreaker"/>.</returns>
        internal CircuitBreaker GetOrCreateCircuitBreaker(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CircuitBreakerSettings circuitBreakerSettings)
        {
            return this.circuitBreakerByGroup.GetOrAdd(commandKey, new Lazy<CircuitBreaker>(() => new CircuitBreaker(commandKey, eventNotifier, circuitBreakerSettings))).Value;
        }
    }
}
