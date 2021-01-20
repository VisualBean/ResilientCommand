using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CircuitBreakerTests
    {
        [TestMethod]
        [ExpectedException(typeof(Polly.CircuitBreaker.BrokenCircuitException))]
        public async Task CircuitBreaker_InSameGroupWithFailures_ThrowsBrokenCircuit()
        {
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 async (ct) => { await Task.Delay(20); return ""; },
                 () => "fallback",
                 groupKey: groupId,
                 timeoutInMiliseconds: 1);

            var command2 = new GenericTestableCommand(
                 (ct) => { throw new Exception(); },
                 () => null,
                 groupKey: groupId,
                 timeoutInMiliseconds: 1);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            var response = await command2.ExecuteAsync(default);
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public async Task CircuitBreaker_InDifferentGroupWithFailures_DoesNotThrow()
        {
            var groupId = Guid.NewGuid().ToString();
            var groupId2 = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                 async (ct) => { await Task.Delay(20); return ""; },
                 () => "fallback",
                 groupKey: groupId,
                 timeoutInMiliseconds: 1);

            var command2 = new GenericTestableCommand(
                 (ct) => { throw new Exception(); },
                 () => null,
                 groupKey: groupId2,
                 timeoutInMiliseconds: 10);

            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);
            await command.ExecuteAsync(default);

            var response = await command2.ExecuteAsync(default);
        }
    }
}
