using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ResilientCommand
{
    public class CircuitBreakerFactory
    {
        private static ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>> circuitBreakerByGroup = new ConcurrentDictionary<CommandKey, Lazy<CircuitBreaker>>();

        internal static CircuitBreaker GetOrCreateCircuitBreaker(CommandKey commandKey, CircuitBreakerSettings circuitBreakerSettings)
        {
            return circuitBreakerByGroup.GetOrAdd(commandKey, new Lazy<CircuitBreaker>(() => new CircuitBreaker(circuitBreakerSettings))).Value;
        }
    }
}
