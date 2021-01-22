using Polly;
using Polly.CircuitBreaker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal class CircuitBreaker
    {
        private readonly bool isEnabled;
        private readonly AsyncCircuitBreakerPolicy circuitbreakerPolicy;
        public CircuitBreaker(CircuitBreakerSettings settings = null)
        {
            settings = settings ?? CircuitBreakerSettings.DefaultCircuitBreakerSettings;
            isEnabled = settings.IsEnabled;

            circuitbreakerPolicy = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: settings.FailureThreshhold,
                samplingDuration: TimeSpan.FromMilliseconds(settings.SamplingDurationMiliseconds),
                minimumThroughput: settings.MinimumThroughput,
                durationOfBreak: TimeSpan.FromMilliseconds(settings.DurationMiliseconds),
                onBreak: (ex, ts) => { Console.WriteLine("Broken"); },
                onReset: () => { }
            );
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            if (!isEnabled)
            {
                return await innerAction(cancellationToken);
            }

            return await circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
