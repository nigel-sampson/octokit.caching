using System;
using System.Collections.Concurrent;

namespace Octokit.Caching
{
    public class NaiveInMemoryCache : ICache
    {
        private ConcurrentDictionary<string, object> items = new ConcurrentDictionary<string, object>();

        public T Get<T>(string key)
        {
            object value;

            return items.TryGetValue(key, out value) ? (T) value : default (T);
        }

        public void Set<T>(string key, T value)
        {
            items[key] = value;
        }
    }
}
