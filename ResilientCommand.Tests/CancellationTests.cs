using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CancellationTests
    {
        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Command_WithCancelledToken_PropagatesCancellation()
        {
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                    async (ct) => { ct.ThrowIfCancellationRequested(); return ""; },
                    () => null,
                    commandKey: groupId);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await command.ExecuteAsync(cts.Token);
        }

    }
}
