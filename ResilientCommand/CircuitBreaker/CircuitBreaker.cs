using Polly;
using Polly.CircuitBreaker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public class CircuitBreaker : ExecutionStrategy
    {
        private readonly CircuitBreakerSettings settings;
        private readonly AsyncCircuitBreakerPolicy circuitbreakerPolicy;
        private readonly CommandKey commandKey;

        public CircuitBreaker(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CircuitBreakerSettings settings = null)
        {
            this.settings = settings ?? CircuitBreakerSettings.DefaultCircuitBreakerSettings;

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
        
        public override async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!this.settings.IsEnabled)
            {
                return await innerAction(cancellationToken);
            }

            if (circuitbreakerPolicy.CircuitState == CircuitState.Open)
            {
                throw new CircuitBrokenException(this.commandKey);
            }

            return await circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
