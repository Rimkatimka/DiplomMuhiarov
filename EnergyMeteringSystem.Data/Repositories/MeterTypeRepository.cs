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
    public class MeterTypeRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterTypeRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            return _context.MeterType
                .Select(m => new DirectoryDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = $"{m.Voltage}В, {m.MaxCurrent}А, класс {m.AccuracyClass}"
                })
                .ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var entity = _context.MeterType.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = $"{entity.Voltage}В, {entity.MaxCurrent}А, класс {entity.AccuracyClass}"
            };
        }

        public void Add(DirectoryDto dto)
        {
            // Для простоты — заглушка
            // В реальности нужно маппить на полную сущность
        }

        public void Update(DirectoryDto dto)
        {
            // Заглушка
        }

        public void Delete(int id)
        {
            var entity = _context.MeterType.Find(id);
            if (entity != null)
            {
                _context.MeterType.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
