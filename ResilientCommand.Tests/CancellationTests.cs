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
        [ExpectedInnerException(typeof(OperationCanceledException))]
        public async Task Command_WithCancelledToken_PropagatesCancellation()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                    async (ct) => { return ""; },
                    () => null,
                    commandKey: cmdKey);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await command.ExecuteAsync(cts.Token);
            // Should throw fallbackNotImplementedException with an innerException of OperationCancelledException.
        }

    }

    public class ExpectedInnerExceptionAttribute : ExpectedExceptionBaseAttribute
    {
        private Type expectedInnerExceptionType;

        public ExpectedInnerExceptionAttribute(Type expectedInnerExceptionType)
        {
            this.expectedInnerExceptionType = expectedInnerExceptionType;
        }

        protected override void Verify(Exception exception)
        {
            var innerType = exception.InnerException?.GetType();
            if (innerType != null && innerType == expectedInnerExceptionType)
            {
                return;
            }

            throw new Exception($"Expected inner exception type: {expectedInnerExceptionType}, got: {innerType}");
        }
    }
}
