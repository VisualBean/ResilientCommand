// <copyright file="Collapser.cs" company="Visualbean">
// Copyright (c) Visualbean. All rights reserved.
// </copyright>

namespace ResilientCommand
{
    public class Collapser : ExecutionStrategy
    {
        private readonly CommandKey commandKey;
        private readonly ResilientCommandEventNotifier eventNotifier;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly CollapserSettings settings;
        private readonly long windowInTicks;
        private object lastResult;
        private long nextRun;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private long windowInTicks;

        /// <summary>
        /// Initializes a new instance of the <see cref="Collapser" /> class.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="eventNotifier">The event notifier.</param>
        /// <param name="settings">The settings.</param>
        public Collapser(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CollapserSettings settings)
        {
            this.settings = settings ?? CollapserSettings.DefaultCollapserSettings;

            this.commandKey = commandKey;
            this.windowInTicks = settings.Window.Ticks;
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
