using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ResilientCommand.Tests
{
    [TestClass]
    public class TimeoutTests
    {
        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task Timeout_WithoutFallback_ThrowsTimeoutException()
        {
            var command = new GenericTestableCommand(
                async (ct) => { await Task.Delay(20); return ""; },
                () => null,
                groupKey: "group1",
                timeoutInMiliseconds: 1);
            await command.ExecuteAsync(default);

        }

        [TestMethod]
        public async Task Timeout_WithFallback_ReturnsFallbackValue()
        {
            string fallbackValue = "fallback";
            var command = new GenericTestableCommand(
                  async (ct) => { await Task.Delay(20); return ""; },
                  () => fallbackValue,
                  groupKey: "group1",
                  timeoutInMiliseconds: 1);
            var response = await command.ExecuteAsync(default);

            Assert.AreEqual(fallbackValue, response);

        }
    }
}
