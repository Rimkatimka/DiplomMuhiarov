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
    public class ReadingStatusRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ReadingStatusRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.ReadingStatus
                .Select(s => new DirectoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.ReadingStatus.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new ReadingStatus
            {
                Name = dto.Name,
                Description = dto.Description,
                Code = dto.Name.ToUpper(),
                ColorHex = "#808080"
            };
            _context.ReadingStatus.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.ReadingStatus.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Description = dto.Description;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.ReadingStatus.Find(id);
            if (entity != null)
            {
                _context.ReadingStatus.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
