using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var s = _context.Street
                .Include("City")
                .FirstOrDefault(x => x.Id == id);

            if (s == null) return null;

            return new StreetDto
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
