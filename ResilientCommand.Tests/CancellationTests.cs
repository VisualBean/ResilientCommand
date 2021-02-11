using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    public class CancellationCommand : ResilientCommand<string>
    {
        protected override async Task<string> RunAsync(CancellationToken cancellationToken)
        {
            return "success";
        }
    }

    [TestClass]
    public class CancellationTests
    {
        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Command_WithCancelledToken_PropagatesCancellation()
        {
            var command = new CancellationCommand();

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await command.ExecuteAsync(cts.Token);
        }
    }
}
