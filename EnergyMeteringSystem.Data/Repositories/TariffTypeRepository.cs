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
            var entity = _context.TariffType.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = $"Зон: {entity.ZoneCount}",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new TariffType
            {
                Name = dto.Name,
                ZoneCount = 1,
                Description = dto.Description
            };
            _context.TariffType.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.TariffType.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Description = dto.Description;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.TariffType.Find(id);
            if (entity != null)
            {
                _context.TariffType.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
