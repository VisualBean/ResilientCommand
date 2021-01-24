using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    public class TestException : Exception
    {
    }
    [TestClass]
    public class CircuitBreakerTests
    {
        static CommandConfiguration circuitBreakerConfigurationLowTimeout = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = GenericTestableCommand.SmallCircuitBreaker;
        });

        static CommandConfiguration circuitBreakerConfigurationDisabled = CommandConfiguration.CreateConfiguration(config =>
       {
           config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
       });

        static CommandConfiguration circuitBreakerConfigurationHigherTimeout = CommandConfiguration.CreateConfiguration(config =>
       {
           config.CircuitBreakerSettings = GenericTestableCommand.SmallCircuitBreaker;
       });

        [TestMethod]
        [ExpectedException(typeof(Polly.CircuitBreaker.BrokenCircuitException))]
        public async Task CircuitBreaker_InSameGroupWithFailures_ThrowsBrokenCircuit()
        {
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new Exception(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfigurationLowTimeout);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new Exception(); },
                 fallbackAction: () => null,
                 commandKey: groupId,
                 config: circuitBreakerConfigurationLowTimeout);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            var response = await command2.ExecuteAsync(default);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task CircuitBreaker_InSameGroupWithFailuresWithDisabledCircuit_DoesNotTripCircuit()
        {
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new Exception(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfigurationDisabled);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => null,
                 commandKey: groupId,
                 config: circuitBreakerConfigurationDisabled);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            var response = await command2.ExecuteAsync(default);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public async Task CircuitBreaker_InDifferentGroupWithFailures_DoesNotThrow()
        {
            var groupId = Guid.NewGuid().ToString();
            var groupId2 = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfigurationLowTimeout);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => null,
                 commandKey: groupId2,
                 config: circuitBreakerConfigurationHigherTimeout);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            var response = await command2.ExecuteAsync(default);
        }
    }
}
