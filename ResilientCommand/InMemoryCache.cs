using System;
using System.Collections.Concurrent;

namespace ResilientCommand
{
    internal class InMemoryCache: ICache
    {
        private static readonly ConcurrentDictionary<string, object> resultCache = new ConcurrentDictionary<string, object>();

        public bool TryAdd<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return resultCache.TryAdd(key, value);
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            var result = resultCache.TryGetValue(key, out object innerValue);

            value = (T)innerValue;
            return result;

        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            resultCache.TryRemove(key, out _);
        }
    }

    public interface ICache
    {
        bool TryAdd<T>(string key, T value);
        
        void Remove(string key);
        
        bool TryGet<T>(string key, out T value);
    }
}
