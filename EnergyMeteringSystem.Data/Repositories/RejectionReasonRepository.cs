using System.Collections.Generic;
using System.Linq;
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
            RejectionReason r = _context.RejectionReason.Find(id);
            return r == null
                ? null
                : new DirectoryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.RequiresComment ? "Требует комментарий" : "Без комментария",
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            RejectionReason entity = new()
            {
                Name = dto.Name,
                RequiresComment = dto.Description?.Contains("Требует") ?? false
            };
            _ = _context.RejectionReason.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(DirectoryDto dto)
        {
            RejectionReason entity = _context.RejectionReason.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                _ = _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            RejectionReason entity = _context.RejectionReason.Find(id);
            if (entity != null)
            {
                _ = _context.RejectionReason.Remove(entity);
                _ = _context.SaveChanges();
            }
        }
    }
}
