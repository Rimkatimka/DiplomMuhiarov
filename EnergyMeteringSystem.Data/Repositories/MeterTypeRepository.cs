using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterTypeRepository : IDirectoryRepository<DirectoryDto>  // ← реализуем интерфейс
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterTypeRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.MeterType
                .Select(m => new { m.Id, m.Name, m.Voltage, m.MaxCurrent, m.AccuracyClass })
                .ToList();

            return data.Select(m => new DirectoryDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = $"{m.Voltage}В, {m.MaxCurrent}А, класс {m.AccuracyClass}",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            MeterType m = _context.MeterType.Find(id);
            return m == null
                ? null
                : new DirectoryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = $"{m.Voltage}В, {m.MaxCurrent}А, класс {m.AccuracyClass}",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            MeterType entity = new()
            {
                Name = dto.Name,
                Voltage = 220,
                MaxCurrent = 40,
                AccuracyClass = "1.0",
                DigitCount = 6,
                DecimalPlaces = 0
            };
            _ = _context.MeterType.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            MeterType entity = _context.MeterType.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            MeterType entity = _context.MeterType.Find(id);
            if (entity != null)
            {
                _ = _context.MeterType.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}