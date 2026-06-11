using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class EnergySourceRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        // Константы для кэширования
        private const string CACHE_KEY_ALL = "EnergySources_All";
        private const string CACHE_KEY_BY_ID = "EnergySource_{0}";
        private const int CACHE_MINUTES = 60; // Источники энергии меняются редко

        // Синхронный (для совместимости)
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ АСИНХРОННЫЙ с кэшированием
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                var data = await Query<EnergySource>()
                    .Select(e => new DirectoryDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Description = e.Code,
                        IsActive = true
                    })
                    .OrderBy(e => e.Name)
                    .ToListAsync(cancellationToken);

                return data;
            }, CACHE_MINUTES);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetById с кэшированием
        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var entity = await Query<EnergySource>()
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

                return entity == null
                    ? null
                    : new DirectoryDto
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Description = entity.Code,
                        IsActive = true
                    };
            }, CACHE_MINUTES);
        }

        // Синхронный GetById (для совместимости)
        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с async и инвалидацией кэша
        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название источника энергии не может быть пустым");

            // Генерируем код, если не указан
            string code = dto.Description;
            if (string.IsNullOrWhiteSpace(code))
            {
                code = dto.Name.Length >= 3
                    ? dto.Name.Substring(0, 3).ToUpper()
                    : dto.Name.ToUpper();
            }

            var entity = new EnergySource
            {
                Name = dto.Name.Trim(),
                Code = code,
                CapacityMW = null
            };

            _context.EnergySource.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();

            AuditLogger.Log("INSERT", "EnergySource", entity.Id, null, new { dto.Name, Code = code });

            return entity.Id;
        }

        // Синхронный Add (для совместимости)
        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Update с async и инвалидацией кэша
        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название источника энергии не может быть пустым");

            var entity = await _context.EnergySource.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            string newCode = dto.Description ?? entity.Code;

            var oldValues = new { entity.Name, entity.Code };
            var newValues = new { dto.Name, Code = newCode };

            entity.Name = dto.Name.Trim();
            entity.Code = newCode;

            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "EnergySource", entity.Id, oldValues, newValues);

            return true;
        }

        // Синхронный Update (для совместимости)
        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Delete с async и проверкой связей
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.EnergySource.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные точки поставки
            bool hasSupplyPoints = await Query<SupplyPoint>()
                .AnyAsync(sp => sp.EnergySourceId == id, cancellationToken);

            if (hasSupplyPoints)
            {
                throw new InvalidOperationException(
                    "Нельзя удалить источник энергии, так как есть связанные точки поставки");
            }

            var oldValues = new { entity.Name, entity.Code };

            _context.EnergySource.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "EnergySource", id, oldValues, null);

            return true;
        }

        // Синхронный Delete (для совместимости)
        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        // ✅ НОВЫЙ МЕТОД: поиск источников энергии
        public async Task<List<DirectoryDto>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(cancellationToken);

            return await Query<EnergySource>()
                .Where(e => e.Name.Contains(searchTerm) ||
                           (e.Code != null && e.Code.Contains(searchTerm)))
                .Take(maxResults)
                .Select(e => new DirectoryDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Code,
                    IsActive = true
                })
                .OrderBy(e => e.Name)
                .ToListAsync(cancellationToken);
        }

        // ✅ НОВЫЙ МЕТОД: проверка существования
        public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = Query<EnergySource>().Where(e => e.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        // ✅ НОВЫЙ МЕТОД: получение с пагинацией
        public async Task<PaginatedResult<DirectoryDto>> GetPaginatedAsync(
            int page,
            int pageSize,
            string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query<EnergySource>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm) ||
                                        (e.Code != null && e.Code.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new DirectoryDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Code,
                    IsActive = true
                })
                .ToListAsync(cancellationToken);

            return new PaginatedResult<DirectoryDto>(items, totalCount, page, pageSize);
        }

        // Приватный метод инвалидации кэша
        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL);
        }
    }
}