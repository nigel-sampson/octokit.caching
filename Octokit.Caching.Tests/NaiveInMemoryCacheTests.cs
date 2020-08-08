using System.Threading.Tasks;
using Xunit;

namespace Octokit.Caching.Tests
{
    public class NaiveInMemoryCacheTests
    {
        [Fact]
        public async Task GetReturnsDefaultWhenMissing()
        {
            var cache = new NaiveInMemoryCache();

            var stringValue = await cache.GetAsync<string>("string");
            var intValue = await cache.GetAsync<int>("int");

            Assert.Null(stringValue);
            Assert.Equal(0, intValue);
        }

        [Fact]
        public async Task SetStoresValue()
        {
            var cache = new NaiveInMemoryCache();

            await cache.SetAsync("string", "test");

            var stringValue = await cache.GetAsync<string>("string");

            Assert.Equal("test", stringValue);
        }

        [Fact]
        public async Task ClearRemovesValues()
        {
            var cache = new NaiveInMemoryCache();

            await cache.SetAsync("string", "test");

            await cache.ClearAsync();

            var stringValue = await cache.GetAsync<string>("string");

            Assert.Null(stringValue);
        }
    }
}