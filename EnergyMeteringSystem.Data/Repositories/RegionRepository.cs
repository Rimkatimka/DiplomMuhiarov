using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class RegionRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public RegionRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<RegionDto> GetAll()
        {
            return _context.Region
                .Select(r => new RegionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code
                })
                .OrderBy(r => r.Name)
                .ToList();
        }
        public void Add(RegionDto dto)
        {
            var entity = new Region
            {
                Name = dto.Name,
                Code = dto.Code
            };
            _context.Region.Add(entity);
            _context.SaveChanges();
        }
    }
}