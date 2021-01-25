using FluentAssertions;
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
        public async Task Command_WithCancelledToken_PropagatesCancellation()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                    async (ct) => { ct.ThrowIfCancellationRequested(); return ""; },
                    () => null,
                    commandKey: cmdKey);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            try
            {
                await command.ExecuteAsync(cts.Token);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType(typeof(FallbackNotImplementedException));
                ex.InnerException.Should().BeOfType(typeof(OperationCanceledException));
            }
            
        }

    }
}
