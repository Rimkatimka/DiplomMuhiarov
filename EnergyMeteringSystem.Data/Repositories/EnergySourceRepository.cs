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
    public class EnergySourceRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public EnergySourceRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.EnergySource
                .Select(e => new DirectoryDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Code,
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.EnergySource.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Code,
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new EnergySource
            {
                Name = dto.Name,
                Code = dto.Description ?? dto.Name.Substring(0, 3).ToUpper(),
                CapacityMW = null
            };
            _context.EnergySource.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.EnergySource.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Code = dto.Description ?? entity.Code;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.EnergySource.Find(id);
            if (entity != null)
            {
                _context.EnergySource.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
