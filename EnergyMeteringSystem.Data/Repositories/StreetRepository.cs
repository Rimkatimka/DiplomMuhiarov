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
    public class StreetRepository : BaseRepository
    {
        public async Task<List<StreetDto>> GetAllAsync(CancellationToken cancellationToken = default)
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
                .ToListAsync(cancellationToken);
        }

        public List<StreetDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<StreetDto>> GetByCityIdAsync(int cityId, CancellationToken cancellationToken = default)
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
                .ToListAsync(cancellationToken);
        }

        public List<StreetDto> GetByCityId(int cityId)
        {
            return GetByCityIdAsync(cityId).Result;
        }

        public async Task<StreetDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var s = await _context.Street
                .Include(x => x.City)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (s == null) return null;

            return new StreetDto
            {
                Id = s.Id,
                Name = s.Name,
                CityId = s.CityId,
                CityName = s.City?.Name ?? "Неизвестно",
                PostalCode = s.PostalCode
            };
        }

        public StreetDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(StreetDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название улицы не может быть пустым");

            var entity = new Street
            {
                Name = dto.Name.Trim(),
                CityId = dto.CityId,
                PostalCode = dto.PostalCode?.Trim()
            };

            _context.Street.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "Street", entity.Id, null, new { dto.Name, dto.CityId });

            return entity.Id;
        }

        public void Add(StreetDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Street.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasObjects = await Query<ConsumptionObject>()
                .AnyAsync(o => o.StreetId == id, cancellationToken);

            if (hasObjects)
            {
                throw new InvalidOperationException("Нельзя удалить улицу, на которой есть объекты");
            }

            var oldValues = new { entity.Name };
            int cityId = entity.CityId;

            _context.Street.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "Street", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }
    }
}