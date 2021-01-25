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
        private readonly CommandKey commandKey;

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
                onBreak: (ex, ts) =>
                {
                    eventNotifier.MarkEvent(ResillientCommandEventType.CircuitBroken, commandKey);
                    throw new CircuitBrokenException(this.commandKey, ex);
                },
                onReset: () => eventNotifier.MarkEvent(ResillientCommandEventType.CircuitReset, commandKey)
            );

            this.commandKey = commandKey;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken = default)
        {
            if (circuitbreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new CircuitBrokenException(this.commandKey);
            }

            return await circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
