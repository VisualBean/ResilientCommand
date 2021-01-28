using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class TimeoutTests
    {
        static CommandConfiguration DisabledTimeout = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(isEnabled: false);
        });

        static CommandConfiguration LowTimeout = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(executionTimeoutInMiliseconds: 1);
        });

        static CommandConfiguration LowTimeoutDisabledFallback = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(executionTimeoutInMiliseconds: 1);
            config.FallbackEnabled = false;
        });

        static CommandConfiguration allButTimeoutDisabled = CommandConfiguration.CreateConfiguration(config =>
       {
           config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(executionTimeoutInMiliseconds: 1);
           config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
           config.FallbackEnabled = false;
       });

        [TestMethod]
        public async Task Timeout_WithFallback_ReturnsFallbackValue()
        {
            string fallbackValue = "fallback";
            var command = new GenericTestableCommand(
                  action: async (ct) => { await Task.Delay(10); return ""; },
                  fallbackAction: () => fallbackValue,
                  config: LowTimeout);
            var response = await command.ExecuteAsync(default);

            Assert.AreEqual(fallbackValue, response);

        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Timeout_WithDisabledFallback_ThrowsTimeoutException()
        {
            var command = new GenericTestableCommand(
                action: async (ct) => { await Task.Delay(10); return ""; },
                fallbackAction: () => null,
                config: LowTimeoutDisabledFallback);

            await command.ExecuteAsync(default);

        }
        [TestMethod]
        public async Task Timeout_WithTimeoutDisabled_NoTimeoutIsExecuted()
        {
            var command = new GenericTestableCommand(
                action: async (ct) => { await Task.Delay(10); return ""; },
                fallbackAction: () => null,
                config: DisabledTimeout);
            await command.ExecuteAsync(default);
        }


        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Timeout_WithDisabledCircuitBreaker_Still_Runs()
        {
            var command = new GenericTestableCommand(
               action: async (ct) => { await Task.Delay(10); return ""; },
               fallbackAction: () => null,
               config: allButTimeoutDisabled);
            await command.ExecuteAsync(default);
        }
    }
}
