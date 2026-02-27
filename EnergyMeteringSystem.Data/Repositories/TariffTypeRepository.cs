using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class TariffTypeRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public TariffTypeRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.TariffType
                .Select(t => new { t.Id, t.Name, t.ZoneCount })
                .ToList();

            return data.Select(t => new DirectoryDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = "Зон: " + t.ZoneCount,
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            TariffType entity = _context.TariffType.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = $"Зон: {entity.ZoneCount}",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            TariffType entity = new()
            {
                Name = dto.Name,
                ZoneCount = 1,
                Description = dto.Description
            };
            _ = _context.TariffType.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            TariffType entity = _context.TariffType.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Description = dto.Description;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            TariffType entity = _context.TariffType.Find(id);
            if (entity != null)
            {
                _ = _context.TariffType.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
