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
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            try
            {
                // 1. Сначала проверяем кэш
                if (_cache.Get(key) is T cached)
                    return cached;

                // 2. Блокировка для предотвращения множественных вызовов factory
                var lockObj = _locks.GetOrAdd(key, new object());

                T result;
                bool lockTaken = false;

                try
                {
                    System.Threading.Monitor.Enter(lockObj, ref lockTaken);

                    // 3. Double-check locking - могло добавиться пока ждали блокировку
                    if (_cache.Get(key) is T doubleCached)
                        return doubleCached;

                    // 4. Вызываем фабрику
                    result = await factory();

                    // 5. Добавляем в кэш
                    if (result != null)
                    {
                        _cache.Set(key, result, DateTimeOffset.Now.AddMinutes(minutes));
                    }
                }
                finally
                {
                    if (lockTaken)
                        System.Threading.Monitor.Exit(lockObj);

                    _locks.TryRemove(key, out _);
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CacheService.GetOrAddAsync error for key '{key}': {ex.Message}");
                // При ошибке просто вызываем фабрику без кэширования
                return await factory();
            }
        }

        public static T GetOrAdd<T>(string key, Func<T> factory, int minutes = 30)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            try
            {
                if (_cache.Get(key) is T cached)
                    return cached;

                var lockObj = _locks.GetOrAdd(key, new object());

                T result;
                bool lockTaken = false;

                try
                {
                    System.Threading.Monitor.Enter(lockObj, ref lockTaken);

                    if (_cache.Get(key) is T doubleCached)
                        return doubleCached;

                    result = factory();

                    if (result != null)
                    {
                        _cache.Set(key, result, DateTimeOffset.Now.AddMinutes(minutes));
                    }
                }
                finally
                {
                    if (lockTaken)
                        System.Threading.Monitor.Exit(lockObj);

                    _locks.TryRemove(key, out _);
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CacheService.GetOrAdd error for key '{key}': {ex.Message}");
                return factory();
            }
        }

        public static void Remove(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    _cache.Remove(key);
                    _locks.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CacheService.Remove error: {ex.Message}");
            }
        }

        public static void Clear()
        {
            try
            {
                foreach (var item in _cache)
                {
                    _cache.Remove(item.Key);
                }
                _locks.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CacheService.Clear error: {ex.Message}");
            }
        }

        public static bool Exists(string key)
        {
            try
            {
                return !string.IsNullOrEmpty(key) && _cache.Get(key) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}