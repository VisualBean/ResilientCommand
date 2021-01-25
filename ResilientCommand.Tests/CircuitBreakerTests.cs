using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CircuitBreakerTests
    {
        static CommandConfiguration circuitBreakerConfiguration = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = GenericTestableCommand.SmallCircuitBreaker;
        });

        static CommandConfiguration circuitBreakerConfigurationDisabled = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
            config.FallbackEnabled = false;
        });

        static CommandConfiguration circuitBreakerConfigurationHigherTimeout = CommandConfiguration.CreateConfiguration(config =>
       {
           config.CircuitBreakerSettings = GenericTestableCommand.SmallCircuitBreaker;
           config.FallbackEnabled = false;
       });

        [TestMethod]

        public async Task CircuitBreaker_InSameGroupWithFailures_ThrowsBrokenCircuit()
        {
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfiguration);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => null,
                 commandKey: groupId,
                 config: circuitBreakerConfiguration);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            try
            {
                await command2.ExecuteAsync(default);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(FallbackNotImplementedException));
                Assert.IsInstanceOfType(ex.InnerException, typeof(CircuitBrokenException));
            } 
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task CircuitBreaker_InSameGroupWithFailuresWithDisabledCircuit_DoesNotTripCircuit()
        {
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfigurationDisabled);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfigurationDisabled);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            var response = await command2.ExecuteAsync(default);
        }

        [TestMethod]
        public async Task CircuitBreaker_InDifferentGroupWithFailures_DoesNotThrow()
        {
            var groupId = Guid.NewGuid().ToString();
            var groupId2 = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId,
                 config: circuitBreakerConfiguration);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: groupId2,
                 config: circuitBreakerConfigurationHigherTimeout);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            try
            {
                var response = await command2.ExecuteAsync(default);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType(typeof(AggregateException));
                ex.InnerException.Should().BeOfType(typeof(TestException));
            }
            
        }
    }
}
