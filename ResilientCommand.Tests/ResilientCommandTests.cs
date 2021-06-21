using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    public class BasicCommand : ResilientCommand<int>
    {
        public BasicCommand() : base()
        {
        }

        public BasicCommand(CircuitBreaker circuitBreaker,
                            ExecutionTimeout executionTimeout,
                            Collapser collapser,
                            Semaphore semaphore,
                            ICache cache) : base(circuitBreaker: circuitBreaker,
                                                 executionTimeout: executionTimeout,
                                                 collapser: collapser,
                                                 semaphore: semaphore,
                                                 cache: cache,
                                                 configuration: CommandConfiguration.CreateConfiguration(c => { c.CollapserSettings.IsEnabled = true; }))
        {

        }

        protected override async Task<int> RunAsync(CancellationToken cancellationToken)
        {
            return 2;
        }
    }

    [TestClass]
    public class ResilientCommandTests
    {
        [TestMethod]
        /// The execution order should be 
        /// Check Cache => Collapse => CircuitBreaker => BulkHead => ExecutionTimeout => Set Cache
        public async Task Command_WithFeaturesEnabled_ExecutesInOrder()
        {
            var tracker = new SequenceTracker();

            var commandKey = new CommandKey(Guid.NewGuid().ToString());
            var mockCache = new Mock<ICache>(MockBehavior.Strict) { CallBase = true };
            
            mockCache.Setup(cache => cache.TryGet(It.IsAny<string>(), out It.Ref<It.IsAnyType>.IsAny))
                .Callback(() => tracker.Next(10));

            mockCache.Setup(cache => cache.TryAdd(It.IsAny<string>(), It.IsAny<It.IsAnyType>()))
                .Callback(() => tracker.Next(60));


            var mockCollapser = new Mock<Collapser>(commandKey, new TestNotifier(), It.IsAny<CollapserSettings>()){ CallBase = true };
            mockCollapser.Setup(collapser => collapser.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<It.IsAnyType>>>(), It.IsAny<CancellationToken>()))
                .Callback(() => tracker.Next(20));

            var mockCircuitBreaker = new Mock<CircuitBreaker>(commandKey, new TestNotifier(), It.IsAny<CircuitBreakerSettings>()){ CallBase = true };
            mockCircuitBreaker.Setup(circuitBreaker => circuitBreaker.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<It.IsAnyType>>>(), It.IsAny<CancellationToken>()))
                .Callback(() => tracker.Next(30));

            var mockSemaphore = new Mock<Semaphore>(commandKey, new TestNotifier(), It.IsAny<SemaphoreSettings>()){ CallBase = true };
            mockSemaphore.Setup(semaphore => semaphore.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<It.IsAnyType>>>(), It.IsAny<CancellationToken>()))
                .Callback(() => tracker.Next(40));

            var mockExecutionTimeout = new Mock<ExecutionTimeout>(commandKey, new TestNotifier(), It.IsAny<ExecutionTimeoutSettings>()){ CallBase = true };
            mockExecutionTimeout.Setup(timeout => timeout.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<It.IsAnyType>>>(), It.IsAny<CancellationToken>()))
                .Callback(() => tracker.Next(50));

            var command = new BasicCommand(mockCircuitBreaker.Object, mockExecutionTimeout.Object, mockCollapser.Object, mockSemaphore.Object, mockCache.Object);

            await command.ExecuteAsync(default);
        }

        [TestMethod]
        public async Task Command_WithDefaultSetup_Executes()
        {
            var command = new BasicCommand();
            var result = await command.ExecuteAsync(default);

            Assert.AreEqual(2, result);
        }
    }
}
