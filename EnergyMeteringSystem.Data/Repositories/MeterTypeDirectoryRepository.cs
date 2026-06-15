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
    public class MeterTypeDirectoryRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var data = await Query<MeterType>()
                .Select(mt => new
                {
                    mt.Id,
                    mt.Name,
                    mt.Voltage,
                    mt.MaxCurrent,
                    mt.AccuracyClass
                })
                .OrderBy(mt => mt.Name)
                .ToListAsync(cancellationToken);

            return data.Select(mt => new DirectoryDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Description = $"{mt.Voltage}В, {mt.MaxCurrent}А, кл.{mt.AccuracyClass}",
                IsActive = true
            }).ToList();
        }

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var mt = await Query<MeterType>()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (mt == null) return null;

            return new DirectoryDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Description = $"{mt.Voltage}В, {mt.MaxCurrent}А, кл.{mt.AccuracyClass}",
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
                throw new ArgumentException("Название типа счетчика не может быть пустым");

            var entity = new MeterType
            {
                Name = dto.Name.Trim(),
                Voltage = 220,
                MaxCurrent = 60,
                AccuracyClass = "1.0",
                DigitCount = 6,
                DecimalPlaces = 1,
                ServiceLifeYears = 32
            };

            _context.MeterType.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "MeterType", entity.Id, null, new { dto.Name });

            return entity.Id;
        }

        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название типа счетчика не может быть пустым");

            var entity = await _context.MeterType.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name };
            var newValues = new { dto.Name };

            entity.Name = dto.Name.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "MeterType", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.MeterType.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasMeters = await Query<Meter>().AnyAsync(m => m.MeterTypeId == id, cancellationToken);
            if (hasMeters)
            {
                throw new InvalidOperationException("Нельзя удалить тип счётчика, который используется");
            }

            var oldValues = new { entity.Name };

            _context.MeterType.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "MeterType", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }
    }
}