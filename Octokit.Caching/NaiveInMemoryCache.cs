using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Octokit.Caching
{
    public class NaiveInMemoryCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public Task<T> GetAsync<T>(string key)
        {
            return _items.TryGetValue(key, out var value) ? Task.FromResult((T) value) : Task.FromResult(default(T));
        }

        public Task SetAsync<T>(string key, T value)
        {
            _items[key] = value;

            return Task.FromResult(true);
        }

        public Task ClearAsync()
        {
            _items.Clear();

            return Task.FromResult(true);
        }
    }
}