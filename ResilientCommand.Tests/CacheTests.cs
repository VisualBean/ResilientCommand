using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public async Task Command_WithCacheKey_ReturnsCachedResult()
        {
            int count = 0;
            var value = "Test";
            var groupId = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                    async (ct) => { count++; return value; },
                    () => null,
                    cacheKey: "cacheKey",
                    commandKey: groupId);

            await command.ExecuteAsync(default);
            var response = await command.ExecuteAsync(default);

            Assert.AreEqual(1, count);
            Assert.AreEqual(value, response);
        }

        [TestMethod]
        public async Task Command_WithCacheKeyInDifferentGroup_DoesNotReturnCachedResult()
        {
            int count = 0;
            var value = "Test";
            var groupId = Guid.NewGuid().ToString();
            var groupId2 = Guid.NewGuid().ToString();

            var command = new GenericTestableCommand(
                    async (ct) => { count++; return value; },
                    () => null,
                    cacheKey: "cacheKey",
                    commandKey: groupId);

            var command2 = new GenericTestableCommand(
                   async (ct) => { count++; return value; },
                   () => null,
                   cacheKey: "cacheKey",
                   commandKey: groupId);

            await command.ExecuteAsync(default);
            var response = await command2.ExecuteAsync(default);

            Assert.AreEqual(2, count);
            Assert.AreEqual(value, response);
        }
    }
}
