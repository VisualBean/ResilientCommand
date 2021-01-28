using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CollapserTests
    {
        static CommandConfiguration allButCollapserDisabled = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(isEnabled: false);
            config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
            config.FallbackEnabled = false;
            config.CollapserSettings = new CollapserSettings(isEnabled: true);
        });

        static CommandConfiguration allDisabled = CommandConfiguration.CreateConfiguration(config =>
        {
            config.ExecutionTimeoutSettings = new ExecutionTimeoutSettings(isEnabled: false);
            config.CircuitBreakerSettings = new CircuitBreakerSettings(isEnabled: false);
            config.FallbackEnabled = false;
            config.CollapserSettings = new CollapserSettings(isEnabled: false);
        });

        [TestMethod]
        public async Task Collapser_WithOtherStrategiesDisabled_CollapsesRequest()
        {
            int i = 0;
            var command = new GenericTestableCommand(
            action: async (ct) => { i++; return i.ToString(); },
            fallbackAction: null,
            
            config: allButCollapserDisabled
            );

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var result = await command.ExecuteAsync(default);

            result.Should().Be("1");
        }

        [TestMethod]
        public async Task Collapser_WithCollapsingDisabled_DoesNotCollapse()
        {
            int i = 0;
            var command = new GenericTestableCommand(
            action: async (ct) => { i++; return i.ToString(); },
            fallbackAction: null,
            config: allDisabled
            );

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var result = await command.ExecuteAsync(default);

            result.Should().Be("4");
        }
    }
}
