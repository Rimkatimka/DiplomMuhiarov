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
    public class ObjectTypeRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            var data = await Query<ObjectType>()
                .Select(o => new { o.Id, o.Name, o.NormConsumption })
                .ToListAsync();

            return data.Select(o => new DirectoryDto
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.NormConsumption.HasValue ? "Норма: " + o.NormConsumption.Value.ToString() : null,
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var entity = await Query<ObjectType>()
                .FirstOrDefaultAsync(o => o.Id == id);

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
            var entity = new ObjectType { Name = dto.Name };
            _context.ObjectType.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "ObjectType", entity.Id, null, new { dto.Name });
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.ObjectType.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };
                var newValues = new { dto.Name };

                entity.Name = dto.Name;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "ObjectType", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            var entity = _context.ObjectType.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };
                _context.ObjectType.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "ObjectType", id, oldValues, null);
            }
        }
    }
}