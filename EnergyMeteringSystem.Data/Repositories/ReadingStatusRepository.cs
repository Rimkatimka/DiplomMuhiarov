using System.Collections.Generic;
using System.Linq;
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
            ReadingStatus entity = _context.ReadingStatus.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            ReadingStatus entity = new()
            {
                Name = dto.Name,
                Description = dto.Description,
                Code = dto.Name.ToUpper(),
                ColorHex = "#808080"
            };
            _ = _context.ReadingStatus.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            ReadingStatus entity = _context.ReadingStatus.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Description = dto.Description;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            ReadingStatus entity = _context.ReadingStatus.Find(id);
            if (entity != null)
            {
                _ = _context.ReadingStatus.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
