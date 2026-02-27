using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class PaymentMethodRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public PaymentMethodRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.PaymentMethod
                .Select(p => new { p.Id, p.Name, p.RequiresCashier })
                .ToList();

            return data.Select(p => new DirectoryDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.RequiresCashier ? "Требуется кассир" : "Без кассира",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            PaymentMethod entity = _context.PaymentMethod.Find(id);
            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.RequiresCashier ? "Требуется кассир" : "Без кассира",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            PaymentMethod entity = new()
            {
                Name = dto.Name,
                RequiresCashier = dto.Description?.Contains("кассир") ?? false
            };
            _ = _context.PaymentMethod.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            PaymentMethod entity = _context.PaymentMethod.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            PaymentMethod entity = _context.PaymentMethod.Find(id);
            if (entity != null)
            {
                _ = _context.PaymentMethod.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
