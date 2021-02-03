using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CircuitBreakerTests
    {
        static CircuitBreakerSettings SmallCircuitBreaker = new CircuitBreakerSettings(failureThreshhold: 0.1, samplingDurationMiliseconds: int.MaxValue, minimumThroughput: 2);

        static CommandConfiguration circuitBreakerConfiguration = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = SmallCircuitBreaker;
        });

        static CommandConfiguration circuitBreakerConfigurationDisabled = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
        });

        static CommandConfiguration circuitBreakerAndFallbackConfigurationDisabled = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
            config.FallbackEnabled = false;
        });

        static CommandConfiguration circuitBreakerConfigurationWithFallbackDisabled = CommandConfiguration.CreateConfiguration(config =>
        {
            config.CircuitBreakerSettings = SmallCircuitBreaker;
            config.FallbackEnabled = false;
        });

        [TestMethod]
        public async Task CircuitBreaker_WithNoFailures_ReturnsResult()
        {
            var value = "Test";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var circuitBreaker = new CircuitBreaker(cmdKey, new TestNotifier(), SmallCircuitBreaker);
            var response = await circuitBreaker.ExecuteAsync((ct) => Task.FromResult(value), default);

            response.Should().Be(value);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task CircuitBreaker_WithFailureWithinCircuitBreaker_DoesNotSwallowException()
        {
            
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var circuitBreaker = new CircuitBreaker(cmdKey, new TestNotifier(), SmallCircuitBreaker);
            await circuitBreaker.ExecuteAsync<string>((ct) => throw new TestException(), default);
        }

        [TestMethod]
        [ExpectedException(typeof(CircuitBrokenException))]
        public async Task CircuitBreaker_WithBrokenCircuit_ThrowsBrokenCircuitException()
        {

            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var circuitBreaker = new CircuitBreaker(cmdKey, new TestNotifier(), SmallCircuitBreaker);

            try
            {
                await circuitBreaker.ExecuteAsync<string>((ct) => throw new TestException(), default);
            }
            catch
            {
                await circuitBreaker.ExecuteAsync<string>((ct) => throw new TestException(), default);
                await circuitBreaker.ExecuteAsync<string>((ct) => throw new TestException(), default);
            }
            await circuitBreaker.ExecuteAsync<string>((ct) => throw new TestException(), default);
        }

        [TestMethod]
        public async Task CircuitBreakerCommand_InDifferentGroupWithFailures_DoesNotThrow()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var cmdKey2 = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: cmdKey,
                 config: circuitBreakerConfiguration);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: cmdKey2,
                 config: circuitBreakerConfigurationWithFallbackDisabled);

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

        [TestMethod]

        public async Task CircuitBreakerCommand_InSameGroupWithFailures_ThrowsBrokenCircuit()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: cmdKey,
                 config: circuitBreakerConfiguration);

            var command2 = new GenericTestableCommand(
                 action: (ct) => { throw new TestException(); },
                 fallbackAction: () => null,
                 commandKey: cmdKey,
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
        public async Task CircuitBreakerCommand_InSameGroupWithFailuresWithDisabledCircuit_DoesNotTripCircuit()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: cmdKey,
                 config: circuitBreakerConfigurationDisabled);

            var command2 = new GenericTestableCommand(
                 action: async (ct) => { throw new TestException(); },
                 fallbackAction: () => "fallback",
                 commandKey: cmdKey,
                 config: circuitBreakerAndFallbackConfigurationDisabled);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            try
            {
                await command2.ExecuteAsync(default);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(AggregateException));
                Assert.IsInstanceOfType(ex.InnerException, typeof(TestException));
            }
        }
    }
}
