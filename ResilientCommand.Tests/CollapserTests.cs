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
        public async Task Collapser_WithCollapsingDisabled_DoesNotCollapse()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            int i = 0;
            var collapser = new Collapser(cmdKey, new TestNotifier(), new CollapserSettings(isEnabled: false));
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            var result = await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);

            result.Should().Be(4);
        }

        [TestMethod]
        public async Task Collapser_WithCollapsingEnabled_CollapsesRequest()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            int i = 0;
            var collapser = new Collapser(cmdKey, new TestNotifier(), new CollapserSettings(isEnabled: true));
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            var result = await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);

            result.Should().Be(1);
        }

        [TestMethod]
        public async Task Collapser_WithRunTimeDisable_DoesNotCollapseRequest()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var settings = new CollapserSettings(isEnabled: true, window: TimeSpan.FromSeconds(1));
            int i = 0;
            var collapser = new Collapser(cmdKey, new TestNotifier(), settings);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            var result = await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            result.Should().Be(1);

            settings.IsEnabled = false;
            result = await collapser.ExecuteAsync((ct) => { i++; return Task.FromResult(i); }, default);
            result.Should().Be(2);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Collapser_WithCancelledToken_Cancels()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var settings = new CollapserSettings(isEnabled: true, window: TimeSpan.FromSeconds(1));
            var collapser = new Collapser(cmdKey, new TestNotifier(), settings);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await collapser.ExecuteAsync<int>(async (token) => 2, cts.Token);
        }
    }
}
