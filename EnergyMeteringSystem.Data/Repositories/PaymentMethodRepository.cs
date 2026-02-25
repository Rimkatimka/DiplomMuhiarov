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
            var entity = _context.PaymentMethod.Find(id);
            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.RequiresCashier ? "Требуется кассир" : "Без кассира",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new PaymentMethod
            {
                Name = dto.Name,
                RequiresCashier = dto.Description?.Contains("кассир") ?? false
            };
            _context.PaymentMethod.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.PaymentMethod.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.PaymentMethod.Find(id);
            if (entity != null)
            {
                _context.PaymentMethod.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
