using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class RegionRepository : BaseRepository
    {
        // Константы для кэширования
        private const string CACHE_KEY_ALL = "Regions_All";
        private const string CACHE_KEY_BY_ID = "Region_{0}";
        private const string CACHE_KEY_BY_NAME = "Region_Name_{0}";
        private const int CACHE_MINUTES = 60; // Регионы меняются редко

        public List<RegionDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetAll с кэшированием
        public async Task<List<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                return await Query<Region>()
                    .Select(r => new RegionDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Code = r.Code
                    })
                    .OrderBy(r => r.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES);
        }

        public RegionDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetById с кэшированием
        public async Task<RegionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var region = await Query<Region>()
                    .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

                if (region == null) return null;

                return new RegionDto
                {
                    Id = region.Id,
                    Name = region.Name,
                    Code = region.Code
                };
            }, CACHE_MINUTES);
        }

        public RegionDto GetByName(string name)
        {
            return GetByNameAsync(name).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetByName с кэшированием
        public async Task<RegionDto> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string cacheKey = string.Format(CACHE_KEY_BY_NAME, name.ToLower());

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var region = await Query<Region>()
                    .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

                if (region == null) return null;

                return new RegionDto
                {
                    Id = region.Id,
                    Name = region.Name,
                    Code = region.Code
                };
            }, CACHE_MINUTES);
        }

        public bool Exists(string name)
        {
            return ExistsAsync(name).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Exists
        public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var query = Query<Region>().Where(r => r.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с async
        public async Task<int> AddAsync(RegionDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название региона не может быть пустым");

            if (await ExistsAsync(dto.Name, null, cancellationToken))
            {
                throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
            }

            var entity = new Region
            {
                Name = dto.Name.Trim(),
                Code = dto.Code?.Trim()
            };
            _context.Region.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();

            AuditLogger.Log("INSERT", "Region", entity.Id, null, new { dto.Name, dto.Code });

            return entity.Id;
        }

        public void Add(RegionDto dto)
        {
            AddAsync(dto).Wait();
        }

        public void Update(RegionDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Update с async
        public async Task<bool> UpdateAsync(RegionDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название региона не может быть пустым");

            var entity = await _context.Region.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            if (entity.Name != dto.Name && await ExistsAsync(dto.Name, dto.Id, cancellationToken))
            {
                throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
            }

            var oldValues = new { entity.Name, entity.Code };
            var newValues = new { dto.Name, dto.Code };

            entity.Name = dto.Name.Trim();
            entity.Code = dto.Code?.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));
            CacheService.Remove(string.Format(CACHE_KEY_BY_NAME, oldValues.Name?.ToLower()));
            CacheService.Remove(string.Format(CACHE_KEY_BY_NAME, dto.Name.ToLower()));

            AuditLogger.Log("UPDATE", "Region", entity.Id, oldValues, newValues);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Delete с async
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Region.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные города
            bool hasCities = await Query<City>()
                .AnyAsync(c => c.RegionId == id, cancellationToken);

            if (hasCities)
            {
                throw new InvalidOperationException("Нельзя удалить регион, в котором есть города");
            }

            var oldValues = new { entity.Name, entity.Code };

            _context.Region.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));
            CacheService.Remove(string.Format(CACHE_KEY_BY_NAME, oldValues.Name?.ToLower()));

            AuditLogger.Log("DELETE", "Region", id, oldValues, null);

            return true;
        }

        // ✅ НОВЫЙ МЕТОД: получение регионов с пагинацией
        public async Task<PaginatedResult<RegionDto>> GetPaginatedAsync(
            int page,
            int pageSize,
            string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query<Region>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.Name.Contains(searchTerm) ||
                                        (r.Code != null && r.Code.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RegionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code
                })
                .ToListAsync(cancellationToken);

            return new PaginatedResult<RegionDto>(items, totalCount, page, pageSize);
        }

        // ✅ НОВЫЙ МЕТОД: поиск регионов
        public async Task<List<RegionDto>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(cancellationToken);

            return await Query<Region>()
                .Where(r => r.Name.Contains(searchTerm) ||
                           (r.Code != null && r.Code.Contains(searchTerm)))
                .Take(maxResults)
                .Select(r => new RegionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code
                })
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        // Приватный метод инвалидации кэша
        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL);
        }
    }
}