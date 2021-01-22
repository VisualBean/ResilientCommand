using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal class ExecutionTimeout : IExecutionStrategy
    {
        private readonly int timeoutInMiliseconds;
        private bool isEnabled;
        public ExecutionTimeout(ExecutionTimeoutSettings settings = null)
        {
            settings = settings ?? ExecutionTimeoutSettings.DefaultExecutionTimeoutSettings;
            isEnabled = settings.IsEnabled;

            if (settings.ExecutionTimeoutInMiliseconds < 0)
            {
                throw new ArgumentException($"{nameof(settings.ExecutionTimeoutInMiliseconds)} must be greater or equal to 0.");
            }
           
            this.timeoutInMiliseconds = (int)settings.ExecutionTimeoutInMiliseconds;
        }
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            if (!isEnabled)
            {
                return await innerAction(cancellationToken);
            }

            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(timeoutInMiliseconds + 100);

            var runTask = innerAction(tokenSource.Token);
            if (await Task.WhenAny(runTask, Task.Delay(timeoutInMiliseconds)) == runTask)
            {
                return runTask.Result;
            }
            else
            {
                throw new TimeoutException("Command timed out.");
            }
        }
    }
}
