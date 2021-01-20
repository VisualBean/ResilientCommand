using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    internal class Timeout : IExecutionStrategy
    {
        private readonly int timeoutInMiliseconds;

        public Timeout(int timeoutInMiliseconds)
        {
            if (timeoutInMiliseconds < 0)
            {
                throw new ArgumentException($"{nameof(timeoutInMiliseconds)} must be greater or equal to 0.");
            }
           
            this.timeoutInMiliseconds = timeoutInMiliseconds;
        }
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
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
