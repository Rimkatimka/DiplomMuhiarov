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
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var data = await Query<MeterStatus>()
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.CanAcceptReadings
                })
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            return data.Select(s => new DirectoryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                IsActive = true
            }).ToList();
        }

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
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
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название статуса не может быть пустым");

            var entity = new MeterStatus
            {
                Name = dto.Name.Trim(),
                CanAcceptReadings = true
            };

            _context.MeterStatus.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "MeterStatus", entity.Id, null, new { dto.Name });

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

            var entity = await _context.MeterStatus.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name };
            var newValues = new { dto.Name };

            entity.Name = dto.Name.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "MeterStatus", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.MeterStatus.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasMeters = await Query<Meter>()
                .AnyAsync(m => m.MeterStatusId == id, cancellationToken);

            if (hasMeters)
            {
                throw new InvalidOperationException("Нельзя удалить статус, который используется счётчиками");
            }

            var oldValues = new { entity.Name };

            _context.MeterStatus.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "MeterStatus", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<List<DirectoryDto>> GetActiveForReadingAsync(CancellationToken cancellationToken = default)
        {
            var data = await Query<MeterStatus>()
                .Where(s => s.CanAcceptReadings == true)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.CanAcceptReadings
                })
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);

            return data.Select(s => new DirectoryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = "Можно вводить показания",
                IsActive = true
            }).ToList();
        }
    }
}