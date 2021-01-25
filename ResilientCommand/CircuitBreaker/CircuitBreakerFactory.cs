using System;
using System.Collections.Concurrent;

namespace ResilientCommand
{
    internal sealed class CircuitBreakerFactory
    {
        private static readonly Lazy<CircuitBreakerFactory>
            instance =
            new Lazy<CircuitBreakerFactory>
                (() => new CircuitBreakerFactory());

        private ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>> circuitBreakerByGroup = new ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>>();

        internal static CircuitBreakerFactory GetInstance() => instance.Value;

        internal CircuitBreaker GetOrCreateCircuitBreaker(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CircuitBreakerSettings circuitBreakerSettings)
        {
            return circuitBreakerByGroup.GetOrAdd(commandKey, new Lazy<CircuitBreaker>(() => new CircuitBreaker(commandKey, eventNotifier, circuitBreakerSettings))).Value;
        }
    }
}
