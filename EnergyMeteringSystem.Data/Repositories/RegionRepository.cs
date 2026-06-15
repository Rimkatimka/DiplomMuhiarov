using EnergyMeteringSystem.Core.Helpers;
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
    public class RegionRepository : BaseRepository
    {
        public async Task<List<RegionDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Query<Region>()
                .Select(r => new RegionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code
                })
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public List<RegionDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<RegionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var region = await Query<Region>()
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (region == null) return null;

            return new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                Code = region.Code
            };
        }

        public RegionDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<RegionDto> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var region = await Query<Region>()
                .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

            if (region == null) return null;

            return new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                Code = region.Code
            };
        }

        public RegionDto GetByName(string name)
        {
            return GetByNameAsync(name).Result;
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var query = Query<Region>().Where(r => r.Name == name);

            if (excludeId.HasValue)
            {
                query = query.Where(r => r.Id != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> AddAsync(RegionDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название региона не может быть пустым");

            if (await ExistsAsync(dto.Name, null, cancellationToken))
            {
                throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
            }

            var entity = new Region
            {
                Name = dto.Name.Trim(),
                Code = dto.Code?.Trim()
            };

            _context.Region.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "Region", entity.Id, null, new { dto.Name, dto.Code });

            return entity.Id;
        }

        public void Add(RegionDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(RegionDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название региона не может быть пустым");

            var entity = await _context.Region.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            if (entity.Name != dto.Name && await ExistsAsync(dto.Name, dto.Id, cancellationToken))
            {
                throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
            }

            var oldValues = new { entity.Name, entity.Code };
            var newValues = new { dto.Name, dto.Code };

            entity.Name = dto.Name.Trim();
            entity.Code = dto.Code?.Trim();

            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "Region", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(RegionDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Region.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasCities = await Query<City>()
                .AnyAsync(c => c.RegionId == id, cancellationToken);

            if (hasCities)
            {
                throw new InvalidOperationException("Нельзя удалить регион, в котором есть города");
            }

            var oldValues = new { entity.Name, entity.Code };

            _context.Region.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "Region", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }
    }
}