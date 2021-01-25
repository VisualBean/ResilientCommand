using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class TimeoutTests
    {
        static CommandConfiguration LowTimeout = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(executionTimeoutInMiliseconds: 1);
        });

        static CommandConfiguration LowTimeoutDisabledFallback = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(executionTimeoutInMiliseconds: 1);
            config.FallbackEnabled = false;
        });

        static CommandConfiguration DisabledTimeout = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(isEnabled: false);
        });

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Timeout_WithoutDisabledFallback_ThrowsTimeoutException()
        {
            var command = new GenericTestableCommand(
                action: async (ct) => { await Task.Delay(10); return ""; },
                fallbackAction: () => null,
                commandKey: "group1",
                config: LowTimeoutDisabledFallback);

            await command.ExecuteAsync(default);

        }

        [TestMethod]
        public async Task Timeout_WithFallback_ReturnsFallbackValue()
        {
            string fallbackValue = "fallback";
            var command = new GenericTestableCommand(
                  action: async (ct) => { await Task.Delay(10); return ""; },
                  fallbackAction: () => fallbackValue,
                  commandKey: "group1",
                  config: LowTimeout);
            var response = await command.ExecuteAsync(default);

            Assert.AreEqual(fallbackValue, response);

        }

        [TestMethod]
        public async Task Timeout_WithTimeoutDisabled_NoTimeoutIsExecuted()
        {
            var command = new GenericTestableCommand(
                action: async (ct) => { await Task.Delay(10); return ""; },
                fallbackAction: () => null,
                commandKey: "group1",
                config: DisabledTimeout);
            await command.ExecuteAsync(default);
        }
    }
}
