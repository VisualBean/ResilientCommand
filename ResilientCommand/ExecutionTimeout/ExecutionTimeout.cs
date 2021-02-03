﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ResilientCommand.Tests")]
namespace ResilientCommand
{
    internal class ExecutionTimeout: IExecutionStrategy
    {
        private readonly CommandKey commandKey;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly int timeoutInMiliseconds;
        public ExecutionTimeout(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, ExecutionTimeoutSettings settings = null)
        {
            settings = settings ?? ExecutionTimeoutSettings.DefaultExecutionTimeoutSettings;

            if (settings.ExecutionTimeoutInMiliseconds < 0)
            {
                throw new ArgumentException($"{nameof(settings.ExecutionTimeoutInMiliseconds)} must be greater or equal to 0.");
            }
           
            this.timeoutInMiliseconds = (int)settings.ExecutionTimeoutInMiliseconds;
            this.commandKey = commandKey;
            this.eventNotifier = eventNotifier;
        }
        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
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
