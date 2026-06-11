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
    public class RejectionReasonRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        private const string CACHE_KEY_ALL = "RejectionReasons_All";
        private const string CACHE_KEY_BY_ID = "RejectionReason_{0}";
        private const int CACHE_MINUTES = 60;

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                var data = await Query<RejectionReason>()
                    .Select(r => new DirectoryDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                        IsActive = true
                    })
                    .OrderBy(r => r.Name)
                    .ToListAsync(cancellationToken);

                return data;
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
                var r = await Query<RejectionReason>()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                return r == null
                    ? null
                    : new DirectoryDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                        IsActive = true
                    };
            }, CACHE_MINUTES);
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название причины отклонения не может быть пустым");

            var entity = new RejectionReason
            {
                Name = dto.Name.Trim(),
                RequiresComment = dto.Description?.Contains("Требует") ?? false
            };
            _context.RejectionReason.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();

            AuditLogger.Log("INSERT", "RejectionReason", entity.Id, null, new { dto.Name });

            return entity.Id;
        }

        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название причины отклонения не может быть пустым");

            var entity = await _context.RejectionReason.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name };
            var newValues = new { dto.Name };

            entity.Name = dto.Name.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "RejectionReason", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.RejectionReason.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные показания
            bool hasReadings = await Query<MeterReading>()
                .AnyAsync(r => r.RejectionReasonId == id, cancellationToken);

            if (hasReadings)
            {
                throw new InvalidOperationException("Нельзя удалить причину отклонения, которая используется в показаниях");
            }

            var oldValues = new { entity.Name };

            _context.RejectionReason.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "RejectionReason", id, oldValues, null);

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