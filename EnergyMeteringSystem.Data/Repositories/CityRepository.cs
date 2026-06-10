using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class CityRepository : BaseRepository
    {
        // Синхронный (для совместимости)
        public List<CityDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ АСИНХРОННЫЙ
        public async Task<List<CityDto>> GetAllAsync()
        {
            return await Query<City>()
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region.Name
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public CityDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<CityDto> GetByIdAsync(int id)
        {
            var c = await Query<City>()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (c == null) return null;

            return new CityDto
            {
                Id = c.Id,
                Name = c.Name,
                RegionId = c.RegionId,
                RegionName = c.Region?.Name
            };
        }

        public List<CityDto> GetByRegionId(int regionId)
        {
            return GetByRegionIdAsync(regionId).Result;
        }

        public async Task<List<CityDto>> GetByRegionIdAsync(int regionId)
        {
            return await Query<City>()
                .Where(c => c.RegionId == regionId)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region.Name
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public void Add(CityDto dto)
        {
            var entity = new City
            {
                Name = dto.Name,
                RegionId = dto.RegionId
            };
            _context.City.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "City", entity.Id, null, new { dto.Name, dto.RegionId });
        }

        public void Delete(int id)
        {
            var entity = _context.City.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };

                _context.City.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "City", id, oldValues, null);
            }
        }
    }
}