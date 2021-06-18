using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    public class FailFastCommand : ResilientCommand<string>
    {
        private readonly bool throwException;

        public FailFastCommand(bool shouldThrow)
        {
            this.throwException = shouldThrow;
        }
        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            if (this.throwException)
            {
                throw new TestException();
            }

            return Task.FromResult("success");
        }
    }

    public class FailFastWithFallbackCommand : ResilientCommand<string>
    {
        private readonly string fallbackValue;
        private readonly bool throwException;
        public readonly CommandConfiguration configuration;

        public FailFastWithFallbackCommand(string fallbackValue, bool shouldThrow, CommandKey key = null, CommandConfiguration configuration = null) : base(commandKey: key, configuration: configuration)
        {
            this.fallbackValue = fallbackValue;
            this.throwException = shouldThrow;
            this.configuration = configuration;
        }
        protected override string Fallback()
        {
            return fallbackValue;
        }

        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            if (this.throwException)
            {
                throw new TestException();
            }

            return Task.FromResult("success");
        }
    }

    [TestClass]
    public partial class FallbackTests
    {
        public FallbackTests()
        {
            EventNotifierFactory.GetInstance().SetEventNotifier(new TestNotifier());
        }

        [TestMethod]
        [ExpectedInnerException(typeof(TestException))]
        public async Task Command_WithoutFallback_ShouldThrow()
        {
            var command = new FailFastCommand(shouldThrow: true);
            await command.ExecuteAsync(default);
        }

        [TestMethod]
        [ExpectedInnerException(typeof(TestException))]
        public async Task Fallback_WithDisabledFallback_FallbackIsSkippedAndThrows()
        {
            var command = new FailFastWithFallbackCommand(
                "fallback",
                shouldThrow: true,
                configuration: CommandConfiguration.CreateConfiguration(c => c.FallbackSettings = new FallbackSettings(false)));

            await command.ExecuteAsync(default);
        }

        [TestMethod]
        public async Task Fallback_WithFallback_ShouldReturnFallback()
        {
            var fallbackValue = "fallback";
            var command = new FailFastWithFallbackCommand(fallbackValue, shouldThrow: true, key: new CommandKey("test"));
            var result = await command.ExecuteAsync(default);

            result.Should().Be(fallbackValue);
        }

        [TestMethod]
        [ExpectedInnerException(typeof(TestException))]
        public async Task Fallback_WithNotImplementedFallback_ShouldThrow()
        {
            var command = new FailFastCommand(shouldThrow: true);

            await command.ExecuteAsync(default);
        }

        [TestMethod]
        [ExpectedInnerException(typeof(TestException))]
        public async Task Fallback_WithRuntimeFallbackDisable_FallbackIsSkippedAndThrows()
        {
            var fallbackValue = "fallback";
            var fallbackSettings = new FallbackSettings(false);
            var configuration = CommandConfiguration.CreateConfiguration(c => c.FallbackSettings = fallbackSettings);

            var command = new FailFastWithFallbackCommand(
                fallbackValue,
                shouldThrow: true,
                configuration: configuration);

            var result = await command.ExecuteAsync(default);
            Assert.AreEqual(fallbackValue, result);

            fallbackSettings.IsEnabled = false;

            await command.ExecuteAsync(default);
        }
    }
}
