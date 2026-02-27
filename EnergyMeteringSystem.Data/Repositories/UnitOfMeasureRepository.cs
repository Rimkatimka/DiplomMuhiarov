using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class UnitOfMeasureRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public UnitOfMeasureRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.UnitOfMeasure
                .Select(u => new { u.Id, u.Name, u.Symbol })
                .ToList();

            return data.Select(u => new DirectoryDto
            {
                Id = u.Id,
                Name = u.Name,
                Description = u.Symbol,
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            UnitOfMeasure entity = _context.UnitOfMeasure.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Symbol,
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            UnitOfMeasure entity = new()
            {
                Name = dto.Name,
                Symbol = dto.Description ?? dto.Name,
                IsDefault = false
            };
            _ = _context.UnitOfMeasure.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            UnitOfMeasure entity = _context.UnitOfMeasure.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Symbol = dto.Description ?? dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            UnitOfMeasure entity = _context.UnitOfMeasure.Find(id);
            if (entity != null && !entity.IsDefault)
            {
                _ = _context.UnitOfMeasure.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
