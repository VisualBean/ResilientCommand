using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class TimeoutTests
    {
        class TimeoutCommand : ResilientCommand<int>
        {
            public TimeoutCommand(CommandConfiguration configuration) : base(configuration: configuration)
            {

            }
            protected override async Task<int> RunAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10);
                return 1;
            }
        }

        [TestMethod]
        public async Task TimeoutCommand_WithLongRunningOperator_TimesOut()
        {
            var settings = new ExecutionTimeoutSettings(executionTimeoutInMilliseconds: 1);
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var timeout = new ExecutionTimeout(cmdKey, new TestNotifier(), settings);
            var command = new TimeoutCommand(CommandConfiguration.CreateConfiguration(config =>
            {
                config.ExecutionTimeoutSettings = settings;
            }));

            await Assert.ThrowsExceptionAsync<TimeoutException>(() => command.ExecuteAsync(default));

        }

        [TestMethod]
        public async Task Timeout_WithRunTimeDisable_NoTimeoutIsExecuted()
        {
            var settings = new ExecutionTimeoutSettings(executionTimeoutInMilliseconds: 1);
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var timeout = new ExecutionTimeout(cmdKey, new TestNotifier(), settings);

            try
            {
                await timeout.ExecuteAsync(async (ct) => { await Task.Delay(10); return 1; }, default);
            }
            catch (TimeoutException)
            {
                settings.IsEnabled = false;
                await timeout.ExecuteAsync(async (ct) => { await Task.Delay(5); return 1; }, default);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Timeout_WithSlowRun_ThrowsTimeoutException()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var timeout = new ExecutionTimeout(cmdKey, new TestNotifier(), new ExecutionTimeoutSettings(executionTimeoutInMilliseconds: 1));
            await timeout.ExecuteAsync(async (ct) => { await Task.Delay(10); return 1; }, default);
        }

        [TestMethod]
        public async Task Timeout_WithTimeoutDisabled_NoTimeoutIsExecuted()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var timeout = new ExecutionTimeout(cmdKey, new TestNotifier(), new ExecutionTimeoutSettings(isEnabled: false, executionTimeoutInMilliseconds: 1));
            await timeout.ExecuteAsync(async (ct) => { await Task.Delay(10); return 1;}, default);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Timeout_WithCancelledToken_Cancels()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var timeout = new ExecutionTimeout(cmdKey, new TestNotifier(), new ExecutionTimeoutSettings(executionTimeoutInMilliseconds: 1));
           

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await timeout.ExecuteAsync(async (ct) => { await Task.Delay(10); return 1; }, cts.Token);
        }
    }
}
