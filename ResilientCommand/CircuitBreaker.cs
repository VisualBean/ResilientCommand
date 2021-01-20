using Polly;
using Polly.CircuitBreaker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal class CircuitBreaker : IExecutionStrategy
    {
        private readonly AsyncCircuitBreakerPolicy circuitbreakerPolicy;
       
        public CircuitBreaker(CircuitBreakerSettings circuitBreakerSettings = null)
        {
            circuitBreakerSettings = circuitBreakerSettings ?? CircuitBreakerSettings.DefaultCircuitBreakerSettings;

            circuitbreakerPolicy = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: circuitBreakerSettings.FailureThreshhold,
                samplingDuration: TimeSpan.FromMilliseconds(circuitBreakerSettings.SamplingDurationMiliseconds),
                minimumThroughput: circuitBreakerSettings.MinimumThroughput,
                durationOfBreak: TimeSpan.FromMilliseconds(circuitBreakerSettings.DurationMiliseconds),
                onBreak: (ex, ts) => { Console.WriteLine("Broken"); },
                onReset: () => { }
            );
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            return await circuitbreakerPolicy.ExecuteAsync(innerAction, cancellationToken);
        }
    }
}
