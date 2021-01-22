using Polly;
using Polly.CircuitBreaker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal class CircuitBreaker
    {
        private readonly AsyncCircuitBreakerPolicy circuitbreakerPolicy;
        public CircuitBreaker(CircuitBreakerSettings settings = null)
        {
            settings = settings ?? CircuitBreakerSettings.DefaultCircuitBreakerSettings;

            circuitbreakerPolicy = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: settings.FailureThreshhold,
                samplingDuration: TimeSpan.FromMilliseconds(settings.SamplingDurationMiliseconds),
                minimumThroughput: settings.MinimumThroughput,
                durationOfBreak: TimeSpan.FromMilliseconds(settings.DurationMiliseconds),
                onBreak: (ex, ts) => { },
                onReset: () => { }
            );
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            return await circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
