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
        public List<StreetDto> GetByCityId(int cityId)
        {
            return _context.Street
                .Where(s => s.CityId == cityId)
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
        public void Add(StreetDto dto)
        {
            var entity = new Street
            {
                Name = dto.Name,
                CityId = dto.CityId,
                PostalCode = dto.PostalCode
            };
            _context.Street.Add(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var entity = _context.Street.Find(id);
            if (entity != null)
            {
                _context.Street.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
