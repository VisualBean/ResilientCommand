using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    public class CircuitBreakerCommand : ResilientCommand<string>
    {
        private bool shouldThrow;

        public CircuitBreakerCommand(CommandKey key, bool shouldThrow) : base(
            commandKey: key,
            configuration: CommandConfiguration.CreateConfiguration(
                c => c.CircuitBreakerSettings = new CircuitBreakerSettings(failureThreshhold: 0.1, samplingDurationMilliseconds: int.MaxValue, minimumThroughput: 2)))
        {
            this.shouldThrow = shouldThrow;
        }

        protected override string Fallback()
        {
            return "fallback";
        }

        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            if (shouldThrow)
            {
                throw new TestException();
            }

            return Task.FromResult("success");
        }
    }

    [TestClass]
    public class CircuitBreakerTests
    {
        static CircuitBreakerSettings SmallCircuitBreaker = new CircuitBreakerSettings(failureThreshhold: 0.1, samplingDurationMilliseconds: int.MaxValue, minimumThroughput: 2);

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
            catch (Exception ex)
            {
                Assert.IsTrue(ex is TestException);
                await circuitBreaker.ExecuteAsync<string>((ct) => throw new TestException(), default);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task CircuitBreaker_WithFailuresWithDisabledCircuit_DoesNotTripCircuit()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var circuit = new CircuitBreaker(cmdKey, new TestNotifier(), settings: new CircuitBreakerSettings(isEnabled: false, failureThreshhold: 0.1, samplingDurationMilliseconds: int.MaxValue, minimumThroughput: 2));

            await circuit.ExecuteAsync<string>((ct) => throw new TestException());
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
        public async Task CircuitBreaker_WithNoFailures_ReturnsResult()
        {
            var value = "Test";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var circuitBreaker = new CircuitBreaker(cmdKey, new TestNotifier(), SmallCircuitBreaker);
            var response = await circuitBreaker.ExecuteAsync((ct) => Task.FromResult(value), default);

            response.Should().Be(value);
        }

        [TestMethod]
        public async Task CircuitBreaker_WithRuntimeDisabledCircuit_DoesNotThrow()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var settings = new CircuitBreakerSettings(isEnabled: false, failureThreshhold: 0.1, samplingDurationMilliseconds: int.MaxValue, minimumThroughput: 2);
            var circuit = new CircuitBreaker(cmdKey, new TestNotifier(), settings);
            try
            {
                await circuit.ExecuteAsync<int>((ct) => throw new TestException());
            }
            catch
            {
                try
                {
                    await circuit.ExecuteAsync<int>((ct) => throw new TestException());
                }
                catch
                {
                }
            }

            settings.IsEnabled = false;
            await circuit.ExecuteAsync<int>((ct) => { return Task.FromResult(1); });

        }

        [TestMethod]
        public async Task CircuitBreakerCommand_InDifferentGroupWithFailures_DoesNotThrow()
        {
            var fallbackValue = "fallback";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var cmdKey2 = new CommandKey(Guid.NewGuid().ToString());


            var command = new CircuitBreakerCommand(cmdKey, shouldThrow: true);
            var command2 = new CircuitBreakerCommand(cmdKey2, shouldThrow: false);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var result = await command.ExecuteAsync(default);
            Assert.AreEqual(fallbackValue, result);

            var response = await command2.ExecuteAsync(default); // Should not go directly to fallback.

            Assert.AreEqual("success", response);

        }

        [TestMethod]

        public async Task CircuitBreakerCommand_InSameGroupWithFailures_ThrowsBrokenCircuit()
        {
            var fallbackValue = "fallback";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new CircuitBreakerCommand(cmdKey, shouldThrow: true);
            var command2 = new CircuitBreakerCommand(cmdKey, shouldThrow: false);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var result = await command.ExecuteAsync(default);

            Assert.AreEqual(fallbackValue, result);
            var response = await command2.ExecuteAsync(default); // Should go directly to fallback.

            Assert.AreEqual(fallbackValue, response);
        }
    }
}
