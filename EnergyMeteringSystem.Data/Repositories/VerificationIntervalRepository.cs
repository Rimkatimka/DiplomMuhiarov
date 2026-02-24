using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return _context.VerificationInterval
                .Include("MeterType")
                .Select(v => new DirectoryDto
                {
                    Id = v.Id,
                    Name = v.MeterType.Name,
                    Description = $"Интервал: {v.Years} лет",
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.VerificationInterval
                .Include("MeterType")
                .FirstOrDefault(v => v.Id == id);

            if (entity == null) return null;

            return new DirectoryDto
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
            var entity = _context.VerificationInterval.Find(dto.Id);
            if (entity != null)
            {
                // Обновление только если нужно
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.VerificationInterval.Find(id);
            if (entity != null)
            {
                _context.VerificationInterval.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
