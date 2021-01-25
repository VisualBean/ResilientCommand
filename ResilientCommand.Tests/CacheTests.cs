using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CacheTests
    {
        private TestNotifier notifier;

        [TestMethod]
        public async Task Command_WithCacheKey_ReturnsCachedResult()
        {
            int count = 0;
            var value = "Test";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                    action: async (ct) => { count++; return value; },
                    fallbackAction: () => null,
                    cacheKey: "cacheKey",
                    commandKey: cmdKey);

            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);

            Assert.AreEqual(1, count);
            Assert.AreEqual(value, response);

            notifier.events[cmdKey].Should().Contain(ResillientCommandEventType.ResponseFromCache);
        }

        [TestMethod]
        public async Task Command_WithCacheKeyInDifferentGroup_DoesNotReturnCachedResult()
        {
            EventNotifierFactory.GetInstance().SetEventNotifier(new TestNotifier());
            var notifier = EventNotifierFactory.GetInstance().GetEventNotifier() as TestNotifier;

            int count = 0;
            var value = "Test";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var cmdKey2 = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                    action: async (ct) => { count++; return value; },
                    fallbackAction: () => null,
                    cacheKey: "cacheKey",
                    commandKey: cmdKey);

            var command2 = new GenericTestableCommand(
                   async (ct) => { count++; return value; },
                   () => null,
                   cacheKey: "cacheKey",
                   commandKey: cmdKey2);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            var response = await command2.ExecuteAsync(default);

            Assert.AreEqual(2, count);
            Assert.AreEqual(value, response);

            notifier.events[cmdKey].Should().Contain(ResillientCommandEventType.ResponseFromCache);
            notifier.events[cmdKey2].Should().NotContain(ResillientCommandEventType.ResponseFromCache);
        }

        [TestMethod]
        public async Task Command_Withfallback_FallbackIsNotCached()
        {
            var fallbackValue = "Fallback";

            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new GenericTestableCommand(
                    action: (ct) => throw new TestException(),
                    fallbackAction: () => fallbackValue,
                    cacheKey: "cacheKey",
                    commandKey: cmdKey);

            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);
            var response2 = await command.ExecuteAsync(default);

            response.Should().Be(fallbackValue);
            response2.Should().Be(fallbackValue);

            notifier.events[cmdKey].Should().Contain(ResillientCommandEventType.Failure);
            notifier.events[cmdKey].Should().NotContain(ResillientCommandEventType.ResponseFromCache);
        }

        [TestInitialize]
        public void init()
        {
            EventNotifierFactory.GetInstance().SetEventNotifier(new TestNotifier());
            this.notifier = EventNotifierFactory.GetInstance().GetEventNotifier() as TestNotifier;
        }
    }
}
