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
    public class ObjectTypeRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ObjectTypeRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.ObjectType
                .Select(o => new DirectoryDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.NormConsumption.HasValue ? $"Норма: {o.NormConsumption}" : null,
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.ObjectType.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.NormConsumption.HasValue ? $"Норма: {entity.NormConsumption}" : null,
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new ObjectType
            {
                Name = dto.Name,
                NormConsumption = null
            };
            _context.ObjectType.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.ObjectType.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.ObjectType.Find(id);
            if (entity != null)
            {
                _context.ObjectType.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
