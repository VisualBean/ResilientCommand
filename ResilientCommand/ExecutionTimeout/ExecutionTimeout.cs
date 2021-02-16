// <copyright file="ExecutionTimeout.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A Timeout decorator implementation.
    /// </summary>
    /// <seealso cref="ResilientCommand.ExecutionDecorator" />
    public class ExecutionTimeout : ExecutionDecorator
    {
        private readonly CommandKey commandKey;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly int timeoutInMilliseconds;
        private ExecutionTimeoutSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionTimeout" /> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="settings">The settings.</param>
        public ExecutionTimeout(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, ExecutionTimeoutSettings settings = null)
        {
            this.settings = settings ?? ExecutionTimeoutSettings.DefaultExecutionTimeoutSettings;
            this.timeoutInMilliseconds = this.settings.ExecutionTimeoutInMilliseconds;
            this.commandKey = commandKey;
            this.eventNotifier = eventNotifier;
        }

        /// <inheritdoc/>
        public override async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!this.settings.IsEnabled)
            {
                return await innerAction(cancellationToken);
            }

            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            tokenSource.CancelAfter(this.timeoutInMilliseconds);

            var runTask = innerAction(tokenSource.Token);
            if (await Task.WhenAny(runTask, Task.Delay(this.timeoutInMilliseconds)) == runTask)
            {
                return runTask.Result;
            }
            else
            {
                this.eventNotifier.RaiseEvent(ResilientCommandEventType.TimedOut, this.commandKey);
                throw new TimeoutException("Command timed out.");
            }
        }
    }
}
