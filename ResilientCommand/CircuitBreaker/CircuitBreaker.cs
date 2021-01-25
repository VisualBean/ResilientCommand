using Polly;
using Polly.CircuitBreaker;
using System;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal class CircuitBreaker
    {
        private readonly AsyncCircuitBreakerPolicy circuitbreakerPolicy;
        public CircuitState State => circuitbreakerPolicy.CircuitState;
        public CircuitBreaker(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CircuitBreakerSettings settings = null)
        {
            settings = settings ?? CircuitBreakerSettings.DefaultCircuitBreakerSettings;

            circuitbreakerPolicy = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: settings.FailureThreshhold,
                samplingDuration: TimeSpan.FromMilliseconds(settings.SamplingDurationMiliseconds),
                minimumThroughput: settings.MinimumThroughput,
                durationOfBreak: TimeSpan.FromMilliseconds(settings.DurationMiliseconds),
                onBreak: (ex, ts) => eventNotifier.markEvent(ResillientCommandEventType.CircuitBroken, commandKey),
                onReset: () => eventNotifier.markEvent(ResillientCommandEventType.CircuitReset, commandKey)
            );
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, Func<TResult> onBrokenCircuit = null, CancellationToken cancellationToken = default)
        {
            if (onBrokenCircuit != null && circuitbreakerPolicy.CircuitState == CircuitState.Open)
            {
                return onBrokenCircuit();
            }

            return await circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
