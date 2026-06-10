using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class EnergySourceRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        // Синхронный (для совместимости)
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ АСИНХРОННЫЙ
        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            var data = await Query<EnergySource>()
                .Select(e => new { e.Id, e.Name, e.Code })
                .ToListAsync();

            return data.Select(e => new DirectoryDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Code,
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var entity = await Query<EnergySource>()
                .FirstOrDefaultAsync(e => e.Id == id);

            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Code,
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new EnergySource
            {
                Name = dto.Name,
                Code = dto.Description ?? dto.Name.Substring(0, 3).ToUpper(),
                CapacityMW = null
            };
            _context.EnergySource.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "EnergySource", entity.Id, null, new { dto.Name });
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.EnergySource.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.Name, entity.Code };
                var newValues = new { dto.Name, Code = dto.Description ?? entity.Code };

                entity.Name = dto.Name;
                entity.Code = dto.Description ?? entity.Code;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "EnergySource", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            var entity = _context.EnergySource.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };

                _context.EnergySource.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "EnergySource", id, oldValues, null);
            }
        }
    }
}