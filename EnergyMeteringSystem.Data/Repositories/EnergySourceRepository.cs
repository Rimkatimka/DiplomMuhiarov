using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class EnergySourceRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public EnergySourceRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.EnergySource
                .Select(e => new { e.Id, e.Name, e.Code })
                .ToList();

            return data.Select(e => new DirectoryDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Code,
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            EnergySource entity = _context.EnergySource.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Code,
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            EnergySource entity = new()
            {
                Name = dto.Name,
                Code = dto.Description ?? dto.Name.Substring(0, 3).ToUpper(),
                CapacityMW = null
            };
            _ = _context.EnergySource.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            EnergySource entity = _context.EnergySource.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Code = dto.Description ?? entity.Code;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            EnergySource entity = _context.EnergySource.Find(id);
            if (entity != null)
            {
                _ = _context.EnergySource.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
