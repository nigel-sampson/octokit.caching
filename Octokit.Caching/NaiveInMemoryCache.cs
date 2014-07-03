using System;
using System.Collections.Generic;

namespace Octokit.Caching
{
    public class NaiveInMemoryCache : ICache
    {
        private IDictionary<string, object> items = new Dictionary<string, object>();

        public T Get<T>(string key)
        {
            return items.ContainsKey(key) ? (T) items[key] : default (T);
        }

        public void Set<T>(string key, T value)
        {
            items[key] = value;
        }
    }
}
