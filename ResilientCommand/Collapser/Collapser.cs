// <copyright file="Collapser.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A collapser implementation.
    /// </summary>
    /// <remarks>
    /// It de-bounces requests based on a time-window.
    /// </remarks>
    /// <seealso cref="ExecutionDecorator" />
    public class Collapser : ExecutionDecorator
    {
        private readonly CommandKey commandKey;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly CollapserSettings settings;
        private readonly long windowInTicks;
        private object lastResult;
        private long nextRun;

        /// <summary>
        /// Initializes a new instance of the <see cref="Collapser" /> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="settings">The settings.</param>
        public Collapser(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CollapserSettings settings = null)
        {
            this.settings = settings ?? CollapserSettings.DefaultCollapserSettings;

            this.commandKey = commandKey;
            this.windowInTicks = this.settings.Window.Ticks;
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

            long requested = DateTime.UtcNow.Ticks;

            try
            {
                await this.semaphore.WaitAsync(cancellationToken);

                if (requested <= this.nextRun)
                {
                    this.eventNotifier.RaiseEvent(ResilientCommandEventType.Collapsed, this.commandKey);
                    return (TResult)this.lastResult;
                }

                this.lastResult = await innerAction(cancellationToken);

                this.nextRun = Math.Max(requested + this.windowInTicks, DateTime.UtcNow.Ticks);
                return (TResult)this.lastResult;
            }
            finally
            {
                this.semaphore.Release();
            }
        }
    }
}
