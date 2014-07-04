using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Octokit.Caching.Tests
{
    [TestClass]
    public class NaiveInMemoryCacheTests
    {
        [TestMethod]
        public void GetReturnsDefaultWhenMissing()
        {
            var cache = new NaiveInMemoryCache();

            var stringValue = cache.Get<string>("string");
            var intValue = cache.Get<int>("int");

            Assert.IsNull(stringValue);
            Assert.AreEqual(0, intValue);
        }

        [TestMethod]
        public void SetStoresValue()
        {
            var cache = new NaiveInMemoryCache();

            cache.Set("string", "test");

            var stringValue = cache.Get<string>("string");

            Assert.AreEqual("test", stringValue);
        }
    }
}
