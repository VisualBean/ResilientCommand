using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand
{
    public class Collapser : ExecutionStrategy
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private CommandKey commandKey;
        private long windowInTicks;
        private ResilientCommandEventNotifier eventNotifier;
        private readonly CollapserSettings settings;
        private object lastResult;
        private long nextRun;

        public Collapser(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CollapserSettings settings)
        {
            this.settings = settings ?? CollapserSettings.DefaultCollapserSettings;

            this.commandKey = commandKey;
            this.windowInTicks = settings.Window.Ticks;
            this.eventNotifier = eventNotifier;
        }

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
                await semaphore.WaitAsync(cancellationToken);

                if (requested <= nextRun)
                {
                    this.eventNotifier.MarkEvent(ResillientCommandEventType.Collapsed, this.commandKey);
                    return (TResult)this.lastResult;
                }

                this.lastResult = await innerAction(cancellationToken);

                this.nextRun = Math.Max(requested + windowInTicks, DateTime.UtcNow.Ticks);
                return (TResult)lastResult;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
