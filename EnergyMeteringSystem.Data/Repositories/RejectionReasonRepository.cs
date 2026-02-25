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
    public class RejectionReasonRepository : IDirectoryRepository<DirectoryDto>
    {
        private readonly EnergyMeteringSystemEntities _context;

        public RejectionReasonRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<DirectoryDto> GetAll()
        {
            var data = _context.RejectionReason
                .Select(r => new { r.Id, r.Name, r.RequiresComment })
                .ToList();

            return data.Select(r => new DirectoryDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            var r = _context.RejectionReason.Find(id);
            if (r == null) return null;

            return new DirectoryDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                IsActive = true
            };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new RejectionReason
            {
                Name = dto.Name,
                RequiresComment = dto.Description?.Contains("Требует") ?? false
            };
            _context.RejectionReason.Add(entity);
            _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.RejectionReason.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.RejectionReason.Find(id);
            if (entity != null)
            {
                _context.RejectionReason.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
