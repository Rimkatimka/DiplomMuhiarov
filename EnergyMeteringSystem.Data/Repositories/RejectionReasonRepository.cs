using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class RejectionReasonRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            var data = await Query<RejectionReason>()
                .Select(r => new { r.Id, r.Name, r.RequiresComment })
                .ToListAsync();

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
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var r = await Query<RejectionReason>()
                .FirstOrDefaultAsync(x => x.Id == id);

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
            var entity = new RejectionReason
            {
                Name = dto.Name,
                RequiresComment = dto.Description?.Contains("Требует") ?? false
            };
            _context.RejectionReason.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "RejectionReason", entity.Id, null, new { dto.Name });
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.RejectionReason.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };
                var newValues = new { dto.Name };

                entity.Name = dto.Name;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "RejectionReason", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            var entity = _context.RejectionReason.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };

                _context.RejectionReason.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "RejectionReason", id, oldValues, null);
            }
        }
    }
}