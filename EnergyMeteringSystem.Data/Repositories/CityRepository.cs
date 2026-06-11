using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class CityRepository : BaseRepository
    {
        // Константы для кэширования
        private const string CACHE_KEY_ALL_CITIES = "AllCities";
        private const string CACHE_KEY_CITIES_BY_REGION = "CitiesByRegion_{0}";
        private const int CACHE_MINUTES = 60;

        // Синхронный (для совместимости)
        public List<CityDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ АСИНХРОННЫЙ с кэшированием
        public async Task<List<CityDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL_CITIES, async () =>
            {
                return await Query<City>()
                    .Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        RegionId = c.RegionId,
                        RegionName = c.Region != null ? c.Region.Name : string.Empty
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetById с кэшированием
        public async Task<CityDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"City_{id}";

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var c = await Query<City>()
                    .Include(c => c.Region)
                    .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (c == null) return null;

                return new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region?.Name ?? string.Empty
                };
            }, CACHE_MINUTES);
        }

        // Синхронный GetById (для совместимости)
        public CityDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetByRegionId с кэшированием
        public async Task<List<CityDto>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_CITIES_BY_REGION, regionId);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                return await Query<City>()
                    .Where(c => c.RegionId == regionId)
                    .Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        RegionId = c.RegionId,
                        RegionName = c.Region != null ? c.Region.Name : string.Empty
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES);
        }

        // Синхронный GetByRegionId (для совместимости)
        public List<CityDto> GetByRegionId(int regionId)
        {
            return GetByRegionIdAsync(regionId).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с инвалидацией кэша
        public async Task<int> AddAsync(CityDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new City
            {
                Name = dto.Name?.Trim(),
                RegionId = dto.RegionId
            };

            _context.City.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache(dto.RegionId);

            AuditLogger.Log("INSERT", "City", entity.Id, null, new { dto.Name, dto.RegionId });

            return entity.Id;
        }

        // Синхронный Add (для совместимости)
        public void Add(CityDto dto)
        {
            AddAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Delete с инвалидацией кэша
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.City.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные улицы
            bool hasStreets = await Query<Street>().AnyAsync(s => s.CityId == id, cancellationToken);
            if (hasStreets)
            {
                throw new System.InvalidOperationException("Нельзя удалить город, в котором есть улицы");
            }

            var oldValues = new { entity.Name };
            int regionId = entity.RegionId;

            _context.City.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache(regionId);
            CacheService.Remove($"City_{id}");

            AuditLogger.Log("DELETE", "City", id, oldValues, null);

            return true;
        }

        // Синхронный Delete (для совместимости)
        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        // ✅ НОВЫЙ МЕТОД: обновление города
        public async Task<bool> UpdateAsync(CityDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.City.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name, entity.RegionId };
            var newValues = new { dto.Name, dto.RegionId };

            entity.Name = dto.Name?.Trim();
            entity.RegionId = dto.RegionId;

            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache(dto.RegionId);
            InvalidateCache(entity.RegionId);
            CacheService.Remove($"City_{dto.Id}");

            AuditLogger.Log("UPDATE", "City", entity.Id, oldValues, newValues);

            return true;
        }

        // ✅ НОВЫЙ МЕТОД: проверка существования города
        public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = Query<City>().Where(c => c.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync(cancellationToken);
        }

        // ✅ НОВЫЙ МЕТОД: получение городов с пагинацией
        public async Task<PaginatedResult<CityDto>> GetPaginatedAsync(
            int page,
            int pageSize,
            int? regionId = null,
            string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query<City>()
                .Include(c => c.Region)
                .AsQueryable();

            if (regionId.HasValue && regionId.Value > 0)
            {
                query = query.Where(c => c.RegionId == regionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region != null ? c.Region.Name : string.Empty
                })
                .ToListAsync(cancellationToken);

            return new PaginatedResult<CityDto>(items, totalCount, page, pageSize);
        }

        // Приватный метод инвалидации кэша
        private void InvalidateCache(int regionId)
        {
            CacheService.Remove(CACHE_KEY_ALL_CITIES);
            CacheService.Remove(string.Format(CACHE_KEY_CITIES_BY_REGION, regionId));
        }
    }
}