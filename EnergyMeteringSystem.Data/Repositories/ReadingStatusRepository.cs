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
    public class ReadingStatusRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            return await Query<ReadingStatus>()
                .Select(s => new DirectoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = true
                })
                .ToListAsync();
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var entity = await Query<ReadingStatus>()
                .FirstOrDefaultAsync(s => s.Id == id);

            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsActive = true
                };
        }

        public void Add(DirectoryDto dto)
        {
            var entity = new ReadingStatus
            {
                Name = dto.Name,
                Description = dto.Description,
                Code = dto.Name.ToUpper(),
                ColorHex = "#808080"
            };
            _context.ReadingStatus.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "ReadingStatus", entity.Id, null, new { dto.Name });
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.ReadingStatus.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.Name, entity.Description };
                var newValues = new { dto.Name, dto.Description };

                entity.Name = dto.Name;
                entity.Description = dto.Description;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "ReadingStatus", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            var entity = _context.ReadingStatus.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };

                _context.ReadingStatus.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "ReadingStatus", id, oldValues, null);
            }
        }
    }
}