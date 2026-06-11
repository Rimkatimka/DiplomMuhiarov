using System;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public static class CacheService
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private static readonly ConcurrentDictionary<string, object> _locks = new();

        public static async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory, int minutes = 30)
        {
            // 1. Сначала проверяем кэш
            if (_cache.Get(key) is T cached)
                return cached;

            // 2. Блокировка для предотвращения множественных вызовов factory
            var lockObj = _locks.GetOrAdd(key, new object());

            lock (lockObj)
            {
                // 3. Double-check locking - могло добавиться пока ждали блокировку
                if (_cache.Get(key) is T doubleCached)
                    return doubleCached;
            }

            // 4. Вызываем фабрику (вне блокировки, но с защитой)
            var data = await factory();

            lock (lockObj)
            {
                // 5. Добавляем в кэш
                _cache.Add(key, data, DateTimeOffset.Now.AddMinutes(minutes));
            }

            return data;
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