using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
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
    public class MeterStatusRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        // Константы для кэширования
        private const string CACHE_KEY_ALL = "MeterStatuses_All";
        private const string CACHE_KEY_BY_ID = "MeterStatus_{0}";
        private const int CACHE_MINUTES = 60; // Справочник меняется редко

        // Синхронный (для совместимости)
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetAll с кэшированием
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                var data = await Query<MeterStatus>()
                    .Select(s => new DirectoryDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                        IsActive = true
                    })
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);

                return data;
            }, CACHE_MINUTES);
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetById с кэшированием
        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var entity = await Query<MeterStatus>()
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (entity == null) return null;

                return new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                    IsActive = true
                };
            }, CACHE_MINUTES);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с async
        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название статуса не может быть пустым");

            var entity = new MeterStatus
            {
                Name = dto.Name.Trim(),
                CanAcceptReadings = true // По умолчанию можно вводить показания
            };

            _context.MeterStatus.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();

            AuditLogger.Log("INSERT", "MeterStatus", entity.Id, null, new { dto.Name });

            return entity.Id;
        }

        // Синхронный Add (для совместимости)
        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Update с async
        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название статуса не может быть пустым");

            var entity = await _context.MeterStatus.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name };
            var newValues = new { dto.Name };

            entity.Name = dto.Name.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "MeterStatus", entity.Id, oldValues, newValues);

            return true;
        }

        // Синхронный Update (для совместимости)
        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Delete с async
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.MeterStatus.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные счетчики
            bool hasMeters = await Query<Meter>()
                .AnyAsync(m => m.MeterStatusId == id, cancellationToken);

            if (hasMeters)
            {
                throw new InvalidOperationException("Нельзя удалить статус, который используется счётчиками");
            }

            var oldValues = new { entity.Name };

            _context.MeterStatus.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "MeterStatus", id, oldValues, null);

            return true;
        }

        // Синхронный Delete (для совместимости)
        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        // ✅ НОВЫЙ МЕТОД: получение активных статусов (где можно вводить показания)
        public async Task<List<DirectoryDto>> GetActiveForReadingAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync("MeterStatuses_Active", async () =>
            {
                var data = await Query<MeterStatus>()
                    .Where(s => s.CanAcceptReadings == true)
                    .Select(s => new DirectoryDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = "Можно вводить показания",
                        IsActive = true
                    })
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);

                return data;
            }, CACHE_MINUTES);
        }

        // ✅ НОВЫЙ МЕТОД: поиск статусов
        public async Task<List<DirectoryDto>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync(cancellationToken);

            return await Query<MeterStatus>()
                .Where(s => s.Name.Contains(searchTerm))
                .Take(maxResults)
                .Select(s => new DirectoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                    IsActive = true
                })
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        // ✅ НОВЫЙ МЕТОД: проверка существования статуса
        public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = Query<MeterStatus>().Where(s => s.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        // Приватный метод инвалидации кэша
        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL);
            CacheService.Remove("MeterStatuses_Active");
        }
    }
}