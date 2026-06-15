using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class CityRepository : BaseRepository
    {
        public async Task<List<CityDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await Query<City>()
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region != null ? c.Region.Name : string.Empty
                })
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public List<CityDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<CityDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var c = await Query<City>()
                .Include(c => c.Region)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (c == null) return null;

            return new CityDto
            {
                Id = c.Id,
                Name = c.Name,
                RegionId = c.RegionId,
                RegionName = c.Region?.Name ?? string.Empty
            };
        }

        public CityDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<List<CityDto>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default)
        {
            return await Query<City>()
                .Where(c => c.RegionId == regionId)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region != null ? c.Region.Name : string.Empty
                })
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public List<CityDto> GetByRegionId(int regionId)
        {
            return GetByRegionIdAsync(regionId).Result;
        }

        public async Task<int> AddAsync(CityDto dto, CancellationToken cancellationToken = default)
        {
            var entity = new City
            {
                Name = dto.Name?.Trim(),
                RegionId = dto.RegionId
            };

            _context.City.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "City", entity.Id, null, new { dto.Name, dto.RegionId });

            return entity.Id;
        }

        public void Add(CityDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.City.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasStreets = await Query<Street>().AnyAsync(s => s.CityId == id, cancellationToken);
            if (hasStreets)
            {
                throw new System.InvalidOperationException("Нельзя удалить город, в котором есть улицы");
            }

            var oldValues = new { entity.Name };
            int regionId = entity.RegionId;

            _context.City.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "City", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<bool> UpdateAsync(CityDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.City.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name, entity.RegionId };
            var newValues = new { dto.Name, dto.RegionId };

            entity.Name = dto.Name?.Trim();
            entity.RegionId = dto.RegionId;

            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "City", entity.Id, oldValues, newValues);

            return true;
        }
    }
}