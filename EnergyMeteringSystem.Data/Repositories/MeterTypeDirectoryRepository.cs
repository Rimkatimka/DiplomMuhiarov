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
    public class MeterTypeDirectoryRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            var data = await Query<MeterType>()
                .Select(mt => new { mt.Id, mt.Name, mt.Voltage, mt.MaxCurrent, mt.AccuracyClass })
                .ToListAsync();

            return data.Select(mt => new DirectoryDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Description = $"{mt.Voltage}В, {mt.MaxCurrent}А, кл.{mt.AccuracyClass}",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var mt = await Query<MeterType>().FirstOrDefaultAsync(m => m.Id == id);
            if (mt == null) return null;

            return new DirectoryDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Description = $"{mt.Voltage}В, {mt.MaxCurrent}А, кл.{mt.AccuracyClass}",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new MeterType
            {
                Name = dto.Name,
                Voltage = 220,
                MaxCurrent = 60,
                AccuracyClass = "1.0",
                DigitCount = 6,
                DecimalPlaces = 1,
                ServiceLifeYears = 32
            };
            _context.MeterType.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.MeterType.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };
                var newValues = new { dto.Name };

                entity.Name = dto.Name;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "MeterType", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            var entity = _context.MeterType.Find(id);
            if (entity != null)
            {
                bool hasMeters = _context.Meter.Any(m => m.MeterTypeId == id);
                if (hasMeters)
                {
                    throw new System.InvalidOperationException("Нельзя удалить тип счётчика, который используется");
                }

                var oldValues = new { entity.Name };

                _context.MeterType.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "MeterType", id, oldValues, null);
            }
        }
    }
}