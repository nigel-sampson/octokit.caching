using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Octokit.Caching
{
    public class NaiveInMemoryCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> items = new ConcurrentDictionary<string, object>();

        public Task<T> GetAsync<T>(string key)
        {
            object value;

            return items.TryGetValue(key, out value) ? Task.FromResult((T) value) : Task.FromResult(default (T));
        }

        public Task SetAsync<T>(string key, T value)
        {
            items[key] = value;

            return Task.FromResult(true);
        }

        public Task ClearAsync()
        {
            items.Clear();

            return Task.FromResult(true);
        }
    }
}
