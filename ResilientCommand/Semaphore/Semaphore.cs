// <copyright file="Semaphore.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The semaphore.
    /// </summary>
    /// <seealso cref="ExecutionDecorator" />
    public class Semaphore : ExecutionDecorator
    {
        private SemaphoreSettings settings;
        private SemaphoreSlim semaphoreSlim;
        private CommandKey commandKey;
        private ResilientCommandEventNotifier eventNotifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="Semaphore"/> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="settings">The settings.</param>
        public Semaphore(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, SemaphoreSettings settings = null)
        {
            this.settings = settings ?? SemaphoreSettings.DefaultSemaphoreSettings;
            this.semaphoreSlim = new SemaphoreSlim(this.settings.MaxParallelism, this.settings.MaxParallelism);
            this.commandKey = commandKey;
            this.eventNotifier = eventNotifier;
        }

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="innerAction">The inner action.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task.
        /// </returns>
        /// <exception cref="SemaphoreRejectedException">Thrown when the queue is full.</exception>
        public override async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (!await this.semaphoreSlim.WaitAsync(0))
                {
                    this.eventNotifier.RaiseEvent(ResilientCommandEventType.SemaphoreRejected, this.commandKey);
                    throw new SemaphoreRejectedException();
                }

                return await innerAction(cancellationToken);
            }
            finally
            {
                try
                {
                    this.semaphoreSlim.Release();
                }
                catch
                {
                }
            }
        }
    }
}
