using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class SemaphoreTests
    {
        [TestMethod]
        [ExpectedException(typeof(SemaphoreRejectedException))]
        public async Task Semaphore_WithFullPool_RejectsExecution()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var semaphore = new Semaphore(cmdKey, new TestNotifier(), new SemaphoreSettings(maxParalellism: 1));

            semaphore.ExecuteAsync(async (ct) => { await Task.Delay(1000); return 2; }, default);
            await semaphore.ExecuteAsync(async (ct) => 2, default);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Semaphore_WithCancelledToken_Cancels()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var semaphore = new Semaphore(cmdKey, new TestNotifier(), SemaphoreSettings.DefaultSemaphoreSettings);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await semaphore.ExecuteAsync<int>(async (token) => 2, cts.Token);
        }
    }
}
