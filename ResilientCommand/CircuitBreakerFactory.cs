using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ResilientCommand
{
    internal sealed class CircuitBreakerFactory
    {
        private static readonly Lazy<CircuitBreakerFactory>
            instance =
            new Lazy<CircuitBreakerFactory>
                (() => new CircuitBreakerFactory());

        private ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>> circuitBreakerByGroup = new ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>>();

        internal static CircuitBreakerFactory Instance => instance.Value;

        internal CircuitBreaker GetOrCreateCircuitBreaker(CommandKey commandKey, CircuitBreakerSettings circuitBreakerSettings)
        {
            return circuitBreakerByGroup.GetOrAdd(commandKey, new Lazy<CircuitBreaker>(() => new CircuitBreaker(circuitBreakerSettings))).Value;
        }
    }
}
