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
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var data = await Query<RejectionReason>()
                .Select(r => new { r.Id, r.Name, r.RequiresComment })
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);

            return data.Select(r => new DirectoryDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                IsActive = true
            }).ToList();
        }

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var r = await Query<RejectionReason>()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return r == null ? null : new DirectoryDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                IsActive = true
            };
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
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

            bool hasReadings = await Query<MeterReading>()
                .AnyAsync(r => r.RejectionReasonId == id, cancellationToken);

            if (hasReadings)
            {
                throw new InvalidOperationException("Нельзя удалить причину отклонения, которая используется в показаниях");
            }

            var oldValues = new { entity.Name };

            _context.RejectionReason.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "RejectionReason", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }
    }
}