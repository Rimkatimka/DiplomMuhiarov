using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public abstract class BaseRepository
    {
        protected readonly EnergyMeteringSystemEntities _context;

        protected BaseRepository()
        {
            _context = new EnergyMeteringSystemEntities();
            _context.Configuration.AutoDetectChangesEnabled = false;
            _context.Configuration.ProxyCreationEnabled = false;
            _context.Configuration.LazyLoadingEnabled = false;
        }

        protected IQueryable<T> Query<T>() where T : class
        {
            return _context.Set<T>().AsNoTracking();
        }

        protected async Task<List<T>> QueryAsync<T>(IQueryable<T> query) where T : class
        {
            return await query.ToListAsync();
        }

        protected async Task<List<T>> QueryPageAsync<T>(IQueryable<T> query, int page, int pageSize) where T : class
        {
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        protected async Task<T> FirstOrDefaultAsync<T>(IQueryable<T> query) where T : class
        {
            return await query.FirstOrDefaultAsync();
        }

        protected async Task<int> CountAsync<T>(IQueryable<T> query) where T : class
        {
            return await query.CountAsync();
        }
    }
}