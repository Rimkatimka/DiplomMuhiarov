using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class VerificationIntervalRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public VerificationIntervalRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.VerificationInterval
                .Include("MeterType")
                .Select(v => new { v.Id, v.MeterType.Name, v.Years })
                .ToList();

            return data.Select(v => new DirectoryDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = "Интервал: " + v.Years + " лет",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            VerificationInterval entity = _context.VerificationInterval
                .Include("MeterType")
                .FirstOrDefault(v => v.Id == id);

            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.MeterType.Name,
                    Description = $"Интервал: {entity.Years} лет",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            // Сложнее, требует MeterTypeId
            // Заглушка
        }

        public void Update(DirectoryDto dto)
        {
            VerificationInterval entity = _context.VerificationInterval.Find(dto.Id);
            if (entity != null)
            {
                // Обновление только если нужно
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            VerificationInterval entity = _context.VerificationInterval.Find(id);
            if (entity != null)
            {
                _ = _context.VerificationInterval.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
