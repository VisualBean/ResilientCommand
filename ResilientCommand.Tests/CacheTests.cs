using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq.Protected;
using System.Linq;

namespace ResilientCommand.Tests
{
    public class CacheCommand : ResilientCommand<string>
    {
        public CacheCommand(CommandKey cmdKey) : base(cmdKey)
        {
        }

        protected override string GetCacheKey()
        {
            return "Key";
        }

        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult("test");
        }
    }

    [TestClass]
    public class CacheTests
    {
        private TestNotifier notifier;

        [TestMethod]
        public async Task Command_WithCacheKey_CachesResult()
        {
            var count = 0;
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var mock = new Mock<CacheCommand>(cmdKey) { CallBase = true, };
            mock.Protected().Setup<Task<string>>("RunAsync", ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count.ToString());
                });

            var command = mock.Object;
            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);
            var response2 = await command.ExecuteAsync(default);

            response.Should().Be("1");
            response.Should().Be(response2);

            notifier.events[cmdKey].Should().Contain(ResilientCommandEventType.ResponseFromCache);
        }

        [TestMethod]
        public async Task Command_WithCacheKey_OnlyCallsRunAsyncOnce()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var mock = new Mock<CacheCommand>(cmdKey) { CallBase = true, };
            var command = mock.Object;

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            mock.Protected().Verify("RunAsync", Times.Once(), ItExpr.IsAny<CancellationToken>());
        }
        [TestMethod]
        public async Task Command_WithCacheKeyInDifferentGroup_DoesNotReturnCachedResult()
        {
            var count = 0;
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var cmdKey2 = new CommandKey(Guid.NewGuid().ToString());

            var mock = new Mock<CacheCommand>(cmdKey) { CallBase = true, };
            mock.Protected().Setup<Task<string>>("RunAsync", ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count.ToString());
                });

            var mock2 = new Mock<CacheCommand>(cmdKey2) { CallBase = true, };
            mock2.Protected().Setup<Task<string>>("RunAsync", ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count.ToString());
                });

            var command = mock.Object;
            var command2 = mock2.Object;

            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default); // Should be cached result
            var response2 = await command2.ExecuteAsync(default); // Should not be be cached result

            response.Should().Be("1");
            response2.Should().Be("2");
            mock.Protected().Verify("RunAsync", Times.Once(), ItExpr.IsAny<CancellationToken>());
            mock2.Protected().Verify("RunAsync", Times.Once(), ItExpr.IsAny<CancellationToken>());

            notifier.events[cmdKey].Should().Contain(ResilientCommandEventType.ResponseFromCache);
            notifier.events[cmdKey2].Should().NotContain(ResilientCommandEventType.ResponseFromCache);
        }

        [TestMethod]
        public async Task Command_WithCacheKeyInSameGroup_CachesResult()
        {
            var count = 0;
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());
            var mock = new Mock<CacheCommand>(cmdKey) { CallBase = true, };
            mock.Protected().Setup<Task<string>>("RunAsync", ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count.ToString());
                });

            var mock2 = new Mock<CacheCommand>(cmdKey) { CallBase = true, };
            mock2.Protected().Setup<Task<string>>("RunAsync", ItExpr.IsAny<CancellationToken>())
                .Returns(() =>
                {
                    count++;
                    return Task.FromResult(count.ToString());
                });

            var command = mock.Object;
            var command2 = mock2.Object;

            var response = await command.ExecuteAsync(default); // Not cached result
            var response2 = await command2.ExecuteAsync(default); // Cached result

            response2.Should().Be("1");
            response2.Should().Be(response);

            notifier.events[cmdKey].Should().Contain(ResilientCommandEventType.ResponseFromCache);
        }

        [TestMethod]
        public async Task Command_WithCacheKeyInSameGroup_OnlyCallsRunAsyncOnce()
        {
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var mock = new Mock<CacheCommand>(cmdKey) { CallBase = true, };
            var mock2 = new Mock<CacheCommand>(cmdKey) { CallBase = true, };

            var command = mock.Object;
            var command2 = mock2.Object;

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default); // Should be cached result
            await command2.ExecuteAsync(default); // Should be cached result

            notifier.events[cmdKey].Where(e => e == ResilientCommandEventType.ResponseFromCache).Should().HaveCount(2);
            mock.Protected().Verify("RunAsync", Times.Once(), ItExpr.IsAny<CancellationToken>());
            mock2.Protected().Verify("RunAsync", Times.Never(), ItExpr.IsAny<CancellationToken>());
        }
        [TestMethod]
        public async Task Command_Withfallback_FallbackIsNotCached()
        {
            var fallbackValue = "fallback";
            var cmdKey = new CommandKey(Guid.NewGuid().ToString());

            var command = new FailingCacheCommand(cmdKey, fallbackValue);

            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);
            var response2 = await command.ExecuteAsync(default);

            response.Should().Be(fallbackValue);
            response2.Should().Be(fallbackValue);

            notifier.events[cmdKey].Should().Contain(ResilientCommandEventType.Failure);
            notifier.events[cmdKey].Should().NotContain(ResilientCommandEventType.ResponseFromCache);
        }

        [TestInitialize]
        public void init()
        {
            EventNotifierFactory.GetInstance().SetEventNotifier(new TestNotifier());
            this.notifier = EventNotifierFactory.GetInstance().GetEventNotifier() as TestNotifier;
        }
    }

    class FailingCacheCommand : ResilientCommand<string>
    {
        public int Count = 0;
        private readonly string fallbackValue;
        public FailingCacheCommand(CommandKey cmdKey, string fallbackValue = null) : base(cmdKey)
        {
            this.fallbackValue = fallbackValue;
        }

        protected override string Fallback()
        {
            return fallbackValue ?? "fallback";
        }

        protected override string GetCacheKey()
        {
            return "Key";
        }

        protected override Task<string> RunAsync(CancellationToken cancellationToken)
        {
            throw new TestException();
        }
    }
}
