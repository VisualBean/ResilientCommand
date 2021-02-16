using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public class ExecutionTimeout : ExecutionStrategy
    {
        private readonly CommandKey commandKey;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly int timeoutInMiliseconds;
        private ExecutionTimeoutSettings settings;

        public ExecutionTimeout(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, ExecutionTimeoutSettings settings = null)
        {
            this.settings = settings ?? ExecutionTimeoutSettings.DefaultExecutionTimeoutSettings;

            if (this.settings.ExecutionTimeoutInMiliseconds < 0)
            {
                throw new ArgumentException($"{nameof(settings.ExecutionTimeoutInMiliseconds)} must be greater or equal to 0.");
            }
           
            this.timeoutInMiliseconds = this.settings.ExecutionTimeoutInMiliseconds;
            this.commandKey = commandKey;
            this.eventNotifier = eventNotifier;
        }
        public override async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            if (!this.settings.IsEnabled)
            {
                return await innerAction(cancellationToken);
            }

            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(timeoutInMiliseconds);

            var runTask = innerAction(tokenSource.Token);
            if (await Task.WhenAny(runTask, Task.Delay(timeoutInMiliseconds)) == runTask)
            {
                return runTask.Result;
            }
            else
            {
                eventNotifier.MarkEvent(ResillientCommandEventType.TimedOut, commandKey);
                throw new TimeoutException("Command timed out.");
            }
        }
    }
}
