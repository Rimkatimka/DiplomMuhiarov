using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterStatusRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            var data = await Query<MeterStatus>()
                .Select(s => new { s.Id, s.Name, s.CanAcceptReadings })
                .ToListAsync();

            return data.Select(s => new DirectoryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var entity = await Query<MeterStatus>()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new MeterStatus
            {
                Name = dto.Name,
                CanAcceptReadings = true
            };
            _context.MeterStatus.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "MeterStatus", entity.Id, null, new { dto.Name });
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.MeterStatus.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };
                var newValues = new { dto.Name };

                entity.Name = dto.Name;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "MeterStatus", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            var entity = _context.MeterStatus.Find(id);
            if (entity != null)
            {
                bool hasMeters = _context.Meter.Any(m => m.MeterStatusId == id);
                if (hasMeters)
                {
                    throw new System.InvalidOperationException("Нельзя удалить статус, который используется счётчиками");
                }

                var oldValues = new { entity.Name };

                _context.MeterStatus.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "MeterStatus", id, oldValues, null);
            }
        }
    }
}