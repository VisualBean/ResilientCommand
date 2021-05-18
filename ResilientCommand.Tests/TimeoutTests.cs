using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class TimeoutTests
    {
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
    }
}
