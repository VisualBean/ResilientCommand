using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public partial class FallbackTests
    {
        public FallbackTests()
        {
             EventNotifierFactory.GetInstance().SetEventNotifier(new TestNotifier());
        }

        [TestMethod]
        public async Task Fallback_WithDisabledFallback_FallbackIsSkippedAndThrows()
        {

            var notifier = EventNotifierFactory.GetInstance().GetEventNotifier() as TestNotifier;
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var command = new GenericTestableCommand(
                commandKey: cmdKey,
                action: async (ct) => { throw new TestException(); },
                fallbackAction: () => "fallback",
                config: CommandConfiguration.CreateConfiguration(config => 
                {
                    config.FallbackEnabled = false;
                }));

            try
            {
                var result = await command.ExecuteAsync(default);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType(typeof(AggregateException));
                ex.InnerException.Should().BeOfType(typeof(TestException));
            }
            
            notifier.events[cmdKey].Should().ContainInOrder(ResillientCommandEventType.Failure, ResillientCommandEventType.FallbackSkipped);
        }

        [TestMethod]
        public async Task Fallback_WithNotImplementedFallback_ShouldThrow()
        {
            var command = new GenericTestableCommand(
                action: (ct) => { throw new TestException(); },
                fallbackAction: () => null,
                config: CommandConfiguration.CreateConfiguration(config =>
                {
                    config.FallbackEnabled = true;
                }));

            try
            {
                await command.ExecuteAsync(default);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType(typeof(FallbackNotImplementedException));
                ex.InnerException.Should().BeOfType(typeof(AggregateException));
                ex.InnerException.InnerException.Should().BeOfType(typeof(TestException));
            }
        }

        [TestMethod]
        public async Task Fallback_WithFallback_ShouldReturnFallback()
        {
            var fallbackValue = "fallback";
            var command = new GenericTestableCommand(
                action: (ct) => { throw new TestException(); },
                fallbackAction: () => fallbackValue,
                config: CommandConfiguration.CreateConfiguration(config =>
                {
                    config.FallbackEnabled = true;
                }));

            var result = await command.ExecuteAsync(default);

            result.Should().Be(fallbackValue);
        }

        [TestMethod]
        public async Task Fallback_WithNoExceptions_ShouldReturnResponse()
        {
            var responseValue = "test";
            var fallbackValue = "fallback";
            var command = new GenericTestableCommand(
                action: (ct) => { return Task.FromResult(responseValue); },
                fallbackAction: () => fallbackValue,
                config: CommandConfiguration.CreateConfiguration(config =>
                {
                    config.FallbackEnabled = true;
                }));

            var result = await command.ExecuteAsync(default);

            result.Should().Be(responseValue);
        }
    }
}
