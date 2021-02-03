using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("ResilientCommand.Tests")]
namespace ResilientCommand
{
    public class Collapser : IExecutionStrategy
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private CommandKey commandKey;
        private long windowInTicks;
        private ResilientCommandEventNotifier eventNotifier;
        private object lastResult;
        private long nextRun;

        public Collapser(CommandKey commandKey, ResilientCommandEventNotifier eventNotifier, CollapserSettings settings)
        {
            this.commandKey = commandKey;
            this.windowInTicks = settings.Window.Ticks;
            this.eventNotifier = eventNotifier;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> innerAction, CancellationToken cancellationToken)
        {
            long requested = DateTime.UtcNow.Ticks;

            try
            {
                await semaphore.WaitAsync();

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
