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
    public class MeterStatusRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterStatusRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.MeterStatus
                .Select(s => new DirectoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                    IsActive = true
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.MeterStatus.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.CanAcceptReadings ? "Можно вводить показания" : "Нельзя вводить показания",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new MeterStatus
            {
                Name = dto.Name,
                CanAcceptReadings = true
            };
            _context.MeterStatus.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.MeterStatus.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.MeterStatus.Find(id);
            if (entity != null)
            {
                _context.MeterStatus.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
