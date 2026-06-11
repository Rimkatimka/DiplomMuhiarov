using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.App.Services
{
    public static class CacheService
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private static readonly ConcurrentDictionary<string, object> _locks = new();

        public static async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, int minutesToCache = 30)
        {
            if (_cache.Get(key) is T cached)
                return cached;

            var lockObj = _locks.GetOrAdd(key, new object());

            lock (lockObj)
            {
                if (_cache.Get(key) is T doubleCached)
                    return doubleCached;
            }
            var data = await factory();

            lock (lockObj)
            {
                _cache.Add(key, data, DateTimeOffset.Now.AddMinutes(minutesToCache));
            }

            return data;
        }

        public static T GetOrAdd<T>(string key, Func<T> factory, int minutesToCache = 30)
        {
            if (_cache.Get(key) is T cached)
                return cached;

            var lockObj = _locks.GetOrAdd(key, new object());

            lock (lockObj)
            {
                if (_cache.Get(key) is T doubleCached)
                    return doubleCached;

                var data = factory();
                _cache.Add(key, data, DateTimeOffset.Now.AddMinutes(minutesToCache));
                return data;
            }
        }

        public static void Remove(string key)
        {
            _cache.Remove(key);
            _locks.TryRemove(key, out _);
        }

        public static void Clear()
        {
            foreach (var item in _cache)
                _cache.Remove(item.Key);
            _locks.Clear();
        }

        public static bool Exists(string key)
        {
            return _cache.Get(key) != null;
        }
    }
}