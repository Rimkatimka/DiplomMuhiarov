using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class StreetRepository : BaseRepository
    {
        public List<StreetDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<StreetDto>> GetAllAsync()
        {
            return await Query<Street>()
                .Include(s => s.City)
                .Select(s => new StreetDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CityId = s.CityId,
                    CityName = s.City.Name,
                    PostalCode = s.PostalCode
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public List<StreetDto> GetByCityId(int cityId)
        {
            return GetByCityIdAsync(cityId).Result;
        }

        public async Task<List<StreetDto>> GetByCityIdAsync(int cityId)
        {
            return await Query<Street>()
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
                .ToListAsync();
        }

        public StreetDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<StreetDto> GetByIdAsync(int id)
        {
            var s = await Query<Street>()
                .Include(s => s.City)
                .FirstOrDefaultAsync(x => x.Id == id);

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

            AuditLogger.Log("INSERT", "Street", entity.Id, null, new { dto.Name, dto.CityId });
        }

        public void Delete(int id)
        {
            var entity = _context.Street.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.Name };

                _context.Street.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "Street", id, oldValues, null);
            }
        }
    }
}