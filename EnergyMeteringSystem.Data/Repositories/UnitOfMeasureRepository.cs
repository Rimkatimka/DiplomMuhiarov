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
    public class UnitOfMeasureRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public UnitOfMeasureRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.UnitOfMeasure
                .Select(u => new DirectoryDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Description = u.Symbol,
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.UnitOfMeasure.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Symbol,
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new UnitOfMeasure
            {
                Name = dto.Name,
                Symbol = dto.Description ?? dto.Name,
                IsDefault = false
            };
            _context.UnitOfMeasure.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.UnitOfMeasure.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Symbol = dto.Description ?? dto.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.UnitOfMeasure.Find(id);
            if (entity != null && !entity.IsDefault)
            {
                _context.UnitOfMeasure.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
