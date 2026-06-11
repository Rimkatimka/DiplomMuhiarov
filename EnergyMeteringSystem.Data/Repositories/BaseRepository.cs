using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Threading;

namespace EnergyMeteringSystem.Data.Repositories
{
    public abstract class BaseRepository : IDisposable
    {
        protected readonly EnergyMeteringSystemEntities _context;
        private bool _disposed = false;

        protected BaseRepository()
        {
            _context = new EnergyMeteringSystemEntities();

            // Оптимизация производительности EF
            _context.Configuration.AutoDetectChangesEnabled = false;
            _context.Configuration.ProxyCreationEnabled = false;
            _context.Configuration.LazyLoadingEnabled = false;

            // Дополнительные оптимизации
            _context.Configuration.ValidateOnSaveEnabled = false;
        }

        protected IQueryable<T> Query<T>() where T : class
        {
            return _context.Set<T>().AsNoTracking();
        }

        // ⚠️ Этот метод избыточен - можно直接用 query.ToListAsync()
        // Оставляю для совместимости, но с пометкой Obsolete
        [Obsolete("Используйте query.ToListAsync() напрямую")]
        protected async Task<List<T>> QueryAsync<T>(IQueryable<T> query) where T : class
        {
            return await query.ToListAsync();
        }

        // ✅ Оптимизированный метод пагинации с возвратом PaginatedList
        protected async Task<PaginatedResult<T>> GetPaginatedResultAsync<T>(
            IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default) where T : class
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 1000) pageSize = 1000; // Ограничение максимума

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<T>(items, totalCount, page, pageSize);
        }

        // ⚠️ Дубликат GetPagedAsync, объединяем
        [Obsolete("Используйте GetPaginatedResultAsync")]
        protected async Task<List<T>> GetPagedAsync<T>(IQueryable<T> query, int page, int pageSize) where T : class
        {
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // ✅ Оптимизированный метод получения предыдущих показаний
        protected async Task<Dictionary<int, MeterReading>> GetPreviousReadingsBatchAsync(
            List<int> meterIds,
            DateTime currentDate,
            CancellationToken cancellationToken = default)
        {
            if (meterIds == null || !meterIds.Any())
                return new Dictionary<int, MeterReading>();

            var previousReadings = await Query<MeterReading>()
                .Where(r => meterIds.Contains(r.MeterId) && r.ReadingDate < currentDate)
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToDictionaryAsync(r => r.MeterId, r => r, cancellationToken);

            return previousReadings;
        }

        // ✅ Универсальный метод для batch получения любых сущностей
        protected async Task<Dictionary<TKey, TEntity>> GetBatchAsync<TEntity, TKey>(
            IQueryable<TEntity> query,
            Func<TEntity, TKey> keySelector,
            IEnumerable<TKey> ids,
            CancellationToken cancellationToken = default) where TEntity : class
        {
            if (ids == null || !ids.Any())
                return new Dictionary<TKey, TEntity>();

            var idList = ids.ToList();
            var items = await query
                .Where(entity => idList.Contains(keySelector(entity)))
                .ToDictionaryAsync(keySelector, entity => entity, cancellationToken);

            return items;
        }

        /// <summary>
        /// Параллельное выполнение нескольких запросов
        /// </summary>
        protected async Task<TResult[]> ParallelQueriesAsync<TResult>(params Func<Task<TResult>>[] queries)
        {
            if (queries == null || !queries.Any())
                return Array.Empty<TResult>();

            var tasks = queries.Select(q => q());
            return await Task.WhenAll(tasks);
        }

        // ✅ Метод для массовой вставки (если нужно)
        protected async Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class
        {
            _context.Set<T>().AddRange(entities);
            await _context.SaveChangesAsync();
        }

        // ✅ Метод для массового обновления через raw SQL (быстрее чем EF)
        protected async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        {
            return await _context.Database.ExecuteSqlCommandAsync(sql, parameters);
        }

        // ✅ Освобождение ресурсов
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }
    }

    // Результат пагинации с информацией для UI
    public class PaginatedResult<T>
    {
        public List<T> Items { get; }
        public int TotalCount { get; }
        public int PageIndex { get; }
        public int PageSize { get; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PaginatedResult(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }
}