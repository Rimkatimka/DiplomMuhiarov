
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public static class CacheService
    {
        private static readonly MemoryCache _cache = MemoryCache.Default;

        public static async Task<T> GetOrAddAsync<T>(string key, System.Func<Task<T>> factory, int minutes = 30)
        {
            if (_cache.Get(key) is T cached)
                return cached;

            var data = await factory();
            _cache.Add(key, data, System.DateTimeOffset.Now.AddMinutes(minutes));
            return data;
        }

        public static void Remove(string key) => _cache.Remove(key);
    }
}