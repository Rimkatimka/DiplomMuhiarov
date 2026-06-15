using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class EnergySourceRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var data = await _context.EnergySource
                .Select(e => new { e.Id, e.Name, e.Code })
                .OrderBy(e => e.Name)
                .ToListAsync(cancellationToken);

            return data.Select(e => new DirectoryDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Code,
                IsActive = true
            }).ToList();
        }

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.EnergySource
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (entity == null) return null;

            return new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Code,
                IsActive = true
            };
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название источника энергии не может быть пустым");

            var entity = new EnergySource
            {
                Name = dto.Name.Trim(),
                Code = dto.Description ?? dto.Name.Substring(0, Math.Min(3, dto.Name.Length)).ToUpper(),
                CapacityMW = null
            };

            _context.EnergySource.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "EnergySource", entity.Id, null, new { dto.Name });

            return entity.Id;
        }

        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название источника энергии не может быть пустым");

            var entity = await _context.EnergySource.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name, entity.Code };
            var newValues = new { dto.Name, Code = dto.Description };

            entity.Name = dto.Name.Trim();
            entity.Code = dto.Description ?? entity.Code;

            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "EnergySource", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.EnergySource.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasSupplyPoints = await _context.SupplyPoint.AnyAsync(sp => sp.EnergySourceId == id, cancellationToken);

            if (hasSupplyPoints)
            {
                throw new InvalidOperationException("Нельзя удалить источник энергии, который используется в точках поставки");
            }

            var oldValues = new { entity.Name };

            _context.EnergySource.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "EnergySource", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }
    }
}