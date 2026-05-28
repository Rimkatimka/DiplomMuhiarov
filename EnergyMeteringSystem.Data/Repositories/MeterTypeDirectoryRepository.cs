using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterTypeDirectoryRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterTypeDirectoryRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            // Сначала получаем данные из БД без форматирования
            var data = _context.MeterType
                .Select(mt => new
                {
                    mt.Id,
                    mt.Name,
                    mt.Voltage,
                    mt.MaxCurrent,
                    mt.AccuracyClass
                })
                .ToList();

            // Теперь форматируем в памяти (здесь можно использовать string.Format или $)
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
            var mt = _context.MeterType.Find(id);
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
                entity.Name = dto.Name;
                _context.SaveChanges();
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

                _context.MeterType.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}