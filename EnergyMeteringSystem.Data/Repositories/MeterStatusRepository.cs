using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterStatusRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterStatusRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.MeterStatus
                .Select(s => new { s.Id, s.Name, s.CanAcceptReadings })
                .ToList();

            return data.Select(s => new DirectoryDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            MeterStatus entity = _context.MeterStatus.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            MeterStatus entity = new()
            {
                Name = dto.Name,
                CanAcceptReadings = true
            };
            _ = _context.MeterStatus.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            MeterStatus entity = _context.MeterStatus.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            MeterStatus entity = _context.MeterStatus.Find(id);
            if (entity != null)
            {
                _ = _context.MeterStatus.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
