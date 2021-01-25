using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
  
    public class GenericTestableCommand : ResilientCommand<string>
    {
        public static CircuitBreakerSettings SmallCircuitBreaker = new CircuitBreakerSettings(failureThreshhold: 0.1, samplingDurationMiliseconds: int.MaxValue, minimumThroughput: 2);

        private readonly Func<CancellationToken, Task<string>> action;
        private readonly Func<string> fallbackAction;
        private readonly string cacheKey;

        public GenericTestableCommand(
                Func<CancellationToken, Task<string>> action,
                Func<string> fallbackAction,
                string commandKey = "Cmd1",
                string cacheKey = null,
                CommandConfiguration config = null) : base(
            new CommandKey(commandKey),
            config
            ) 
        {
            this.action = action;
            this.fallbackAction = fallbackAction;
            this.cacheKey = cacheKey;
        }

        protected override string GetCacheKey() => cacheKey;

        protected override async Task<string> RunAsync(CancellationToken cancellationToken) => await action(cancellationToken);

        protected override string Fallback() => fallbackAction();
    }
}
