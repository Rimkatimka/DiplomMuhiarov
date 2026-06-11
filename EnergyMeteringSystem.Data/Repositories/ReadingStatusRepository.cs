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
    public class ReadingStatusRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        private const string CACHE_KEY_ALL = "ReadingStatuses_All";
        private const string CACHE_KEY_BY_ID = "ReadingStatus_{0}";
        private const int CACHE_MINUTES = 60;

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                return await Query<ReadingStatus>()
                    .Select(s => new DirectoryDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        IsActive = true
                    })
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES);
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var entity = await Query<ReadingStatus>()
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                return entity == null
                    ? null
                    : new DirectoryDto
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        Description = entity.Description,
                        IsActive = true
                    };
            }, CACHE_MINUTES);
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название статуса не может быть пустым");

            var entity = new ReadingStatus
            {
                Name = dto.Name.Trim(),
                Description = dto.Description,
                Code = dto.Name.Trim().ToUpper().Replace(" ", "_"),
                ColorHex = "#808080"
            };
            _context.ReadingStatus.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();

            AuditLogger.Log("INSERT", "ReadingStatus", entity.Id, null, new { dto.Name });

            return entity.Id;
        }

        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название статуса не может быть пустым");

            var entity = await _context.ReadingStatus.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name, entity.Description };
            var newValues = new { dto.Name, dto.Description };

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description;
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "ReadingStatus", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ReadingStatus.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные показания
            bool hasReadings = await Query<MeterReading>()
                .AnyAsync(r => r.ReadingStatusId == id, cancellationToken);

            if (hasReadings)
            {
                throw new InvalidOperationException("Нельзя удалить статус, который используется в показаниях");
            }

            var oldValues = new { entity.Name };

            _context.ReadingStatus.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "ReadingStatus", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL);
        }
    }
}