using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class StreetRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public StreetRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<StreetDto> GetAll()
        {
            return _context.Street
                .Include("City")
                .Select(s => new StreetDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CityId = s.CityId,
                    CityName = s.City.Name,
                    PostalCode = s.PostalCode
                })
                .OrderBy(s => s.Name)
                .ToList();
        }

        public StreetDto GetById(int id)
        {
            Street s = _context.Street
                .Include("City")
                .FirstOrDefault(x => x.Id == id);

            return s == null
                ? null
                : new StreetDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CityId = s.CityId,
                    CityName = s.City.Name,
                    PostalCode = s.PostalCode
                };
        }
    }
}
