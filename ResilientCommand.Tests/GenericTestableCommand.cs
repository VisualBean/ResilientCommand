using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    public class GenericTestableCommand : ResilientCommand<string>
    {
        private readonly Func<CancellationToken, Task<string>> action;
        private readonly Func<string> fallbackAction;
        private readonly string cacheKey;

        public GenericTestableCommand(
                Func<CancellationToken, Task<string>> action,
                Func<string> fallbackAction,
                string groupKey,
                int timeoutInMiliseconds,
                string cacheKey = null) : base(
            new GroupKey(groupKey),
            timeoutInMiliseconds: timeoutInMiliseconds,
            circuitBreakerSettings: new CircuitBreakerSettings(failureThreshhold: 0.1, samplingDurationMiliseconds: 20, minimumThroughput: 2)) 
        {
            this.action = action;
            this.fallbackAction = fallbackAction;
            this.cacheKey = cacheKey;
        }

        public override string GetCacheKey() => cacheKey;

        public override async Task<string> RunAsync(CancellationToken cancellationToken) => await action(cancellationToken);

        public override string Fallback() => fallbackAction();
    }
}
