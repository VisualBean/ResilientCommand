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
        [TestMethod]
        public async Task Collapser_WithCollapsingEnabled_CollapsesRequest()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            int i = 0;
            var collapser = new Collapser(cmdKey, new TestNotifier(), new CollapserSettings(isEnabled: true));
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            var result = await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);

            result.Should().Be(1);
        }

        [TestMethod]
        public async Task Collapser_WithRunTimeDisable_DoesNotCollapseRequest()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var settings = new CollapserSettings(isEnabled: true, window: TimeSpan.FromSeconds(1));
            int i = 0;
            var collapser = new Collapser(cmdKey, new TestNotifier(), settings);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            var result = await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            result.Should().Be(1);

            settings.IsEnabled = false;
            result = await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            result.Should().Be(2);
        }

        [TestMethod]
        public async Task Collapser_WithCollapsingDisabled_DoesNotCollapse()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            int i = 0;
            var collapser = new Collapser(cmdKey, new TestNotifier(), new CollapserSettings(isEnabled: false));
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);
            var result = await collapser.ExecuteAsync(async (ct) => { i++; return i; }, default);

            result.Should().Be(4);
        }
    }
}
