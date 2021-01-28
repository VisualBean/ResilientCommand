using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
  
    public class GenericTestableCommand : ResilientCommand<string>
    {
        public static CircuitBreakerSettings SmallCircuitBreaker = new CircuitBreakerSettings(failureThreshhold: 0.1, samplingDurationMiliseconds: int.MaxValue, minimumThroughput: 2);

        private readonly Func<CancellationToken, Task<string>> action;
        private readonly string cacheKey;
        private readonly Func<string> fallbackAction;

        public GenericTestableCommand(
                Func<CancellationToken, Task<string>> action,
                Func<string> fallbackAction = null,
                CommandKey commandKey = null,
                string cacheKey = null,
                CommandConfiguration config = null) : base(
            commandKey,
            config
            ) 
        {
            this.action = action;
            this.fallbackAction = fallbackAction;
            this.cacheKey = cacheKey;
        }

        protected override string Fallback() => fallbackAction();

        protected override string GetCacheKey() => cacheKey;

        protected override async Task<string> RunAsync(CancellationToken cancellationToken) => await action(cancellationToken);
    }
}
