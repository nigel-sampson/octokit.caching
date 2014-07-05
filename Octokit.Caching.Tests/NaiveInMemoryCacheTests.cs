using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Octokit.Caching.Tests
{
    [TestClass]
    public class NaiveInMemoryCacheTests
    {
        [TestMethod]
        public async Task GetReturnsDefaultWhenMissing()
        {
            var cache = new NaiveInMemoryCache();

            var stringValue = await cache.GetAsync<string>("string");
            var intValue = await cache.GetAsync<int>("int");

            Assert.IsNull(stringValue);
            Assert.AreEqual(0, intValue);
        }

        [TestMethod]
        public async Task SetStoresValue()
        {
            var cache = new NaiveInMemoryCache();

            await cache.SetAsync("string", "test");

            var stringValue = await cache.GetAsync<string>("string");

            Assert.AreEqual("test", stringValue);
        }
    }
}
