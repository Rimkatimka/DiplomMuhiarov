using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class CityRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public CityRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<CityDto> GetAll()
        {
            return _context.City
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region.Name
                })
                .OrderBy(c => c.Name)
                .ToList();
        }

        public CityDto GetById(int id)
        {
            var c = _context.City.Find(id);
            if (c == null) return null;

            return new CityDto
            {
                Id = c.Id,
                Name = c.Name,
                RegionId = c.RegionId,
                RegionName = c.Region?.Name
            };
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
        }
        public List<CityDto> GetByRegionId(int regionId)
        {
            return _context.City
                .Where(c => c.RegionId == regionId)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    RegionId = c.RegionId,
                    RegionName = c.Region.Name
                })
                .OrderBy(c => c.Name)
                .ToList();
        }

        public void Delete(int id)
        {
            var entity = _context.City.Find(id);
            if (entity != null)
            {
                _context.City.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}