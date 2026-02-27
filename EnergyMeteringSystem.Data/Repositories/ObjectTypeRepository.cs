using System.Collections.Generic;
using System.Linq;
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
            // Сначала загружаем данные из БД
            var data = _context.ObjectType
                .Select(o => new
                {
                    o.Id,
                    o.Name,
                    o.NormConsumption
                })
                .ToList(); // ← выполняем запрос

            // Теперь форматируем в памяти
            return data.Select(o => new DirectoryDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.NormConsumption.HasValue
                    ? "Норма: " + o.NormConsumption.Value.ToString()
                    : null,
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            ObjectType entity = _context.ObjectType.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.NormConsumption.HasValue ? $"Норма: {entity.NormConsumption}" : null,
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            ObjectType entity = new()
            {
                Name = dto.Name,
                NormConsumption = null
            };
            _ = _context.ObjectType.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            ObjectType entity = _context.ObjectType.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            ObjectType entity = _context.ObjectType.Find(id);
            if (entity != null)
            {
                _ = _context.ObjectType.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
