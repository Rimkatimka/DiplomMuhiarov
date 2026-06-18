using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ConsumptionObjectRepository : BaseRepository, IConsumptionObjectRepository
    {
        public async Task<List<ConsumptionObjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ConsumptionObject
                    .Include(o => o.Street)
                    .Include(o => o.Street.City)
                    .Include(o => o.Street.City.Region)
                    .Include(o => o.ObjectType)
                    .Select(o => new ConsumptionObjectDto
                    {
                        Id = o.Id,
                        StreetId = o.StreetId,
                        Street = o.Street.Name,
                        City = o.Street.City.Name,
                        CityId = o.Street.City.Id,
                        Region = o.Street.City.Region.Name,
                        RegionId = o.Street.City.Region.Id,
                        HouseNumber = o.HouseNumber,
                        ApartmentNumber = o.ApartmentNumber,
                        ObjectTypeId = o.ObjectTypeId,
                        ObjectTypeName = o.ObjectType.Name,
                        TotalArea = o.TotalArea,
                        ResidentCount = o.ResidentCount,
                        NormConsumption = o.ObjectType.NormConsumption  // ← ДОБАВЛЕНО
                    })
                    .OrderBy(o => o.City)
                    .ThenBy(o => o.Street)
                    .ThenBy(o => o.HouseNumber)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync() ERROR: {ex.Message}");
                return new List<ConsumptionObjectDto>();
            }
        }

        public List<ConsumptionObjectDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<ConsumptionObjectDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Query<ConsumptionObject>()
                .Where(o => o.Id == id)
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    StreetId = o.StreetId,
                    Street = o.Street.Name,
                    City = o.Street.City.Name,
                    CityId = o.Street.City.Id,
                    Region = o.Street.City.Region.Name,
                    RegionId = o.Street.City.Region.Id,
                    HouseNumber = o.HouseNumber,
                    ApartmentNumber = o.ApartmentNumber,
                    ObjectTypeId = o.ObjectTypeId,
                    ObjectTypeName = o.ObjectType.Name,
                    TotalArea = o.TotalArea,
                    ResidentCount = o.ResidentCount,
                    NormConsumption = o.ObjectType.NormConsumption  // ← ДОБАВЛЕНО
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public ConsumptionObjectDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] AddAsync НАЧАЛО");

            var entity = new ConsumptionObject
            {
                StreetId = dto.StreetId,
                HouseNumber = dto.HouseNumber?.Trim(),
                ApartmentNumber = dto.ApartmentNumber?.Trim(),
                ObjectTypeId = dto.ObjectTypeId,
                TotalArea = dto.TotalArea,
                ResidentCount = dto.ResidentCount
            };

            _context.ConsumptionObject.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "ConsumptionObject", entity.Id, null, new { dto.Address });

            return entity.Id;
        }

        public void Add(ConsumptionObjectDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (entity == null) return false;

            bool hasMeters = await _context.Meter
                .AnyAsync(m => m.ConsumptionObjectId == id, cancellationToken);

            if (hasMeters)
            {
                throw new InvalidOperationException("Нельзя удалить объект, у которого есть счетчики");
            }

            var oldValues = new { entity.HouseNumber, entity.ApartmentNumber };

            _context.ConsumptionObject.Remove(entity);
            var result = await _context.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                AuditLogger.Log("DELETE", "ConsumptionObject", id, oldValues, null);
            }

            return result > 0;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<bool> UpdateAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject
                .FirstOrDefaultAsync(o => o.Id == dto.Id, cancellationToken);

            if (entity == null) return false;

            entity.StreetId = dto.StreetId;
            entity.HouseNumber = dto.HouseNumber?.Trim();
            entity.ApartmentNumber = dto.ApartmentNumber?.Trim();
            entity.ObjectTypeId = dto.ObjectTypeId;
            entity.TotalArea = dto.TotalArea;
            entity.ResidentCount = dto.ResidentCount;

            var result = await _context.SaveChangesAsync(cancellationToken);

            if (result > 0)
            {
                AuditLogger.Log("UPDATE", "ConsumptionObject", dto.Id, null, new { dto.Address });
            }

            return result > 0;
        }

        public void Update(ConsumptionObjectDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<List<ConsumptionObjectDto>> GetFilteredAsync(int? regionId = null, int? cityId = null, int? streetId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.ConsumptionObject
                .Include(o => o.Street)
                .Include(o => o.Street.City)
                .Include(o => o.Street.City.Region)
                .Include(o => o.ObjectType)
                .AsQueryable();

            if (regionId.HasValue && regionId.Value > 0)
            {
                query = query.Where(o => o.Street.City.RegionId == regionId.Value);
            }

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(o => o.Street.CityId == cityId.Value);
            }

            if (streetId.HasValue && streetId.Value > 0)
            {
                query = query.Where(o => o.StreetId == streetId.Value);
            }

            return await query
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    StreetId = o.StreetId,
                    Street = o.Street.Name,
                    City = o.Street.City.Name,
                    CityId = o.Street.City.Id,
                    Region = o.Street.City.Region.Name,
                    RegionId = o.Street.City.Region.Id,
                    HouseNumber = o.HouseNumber,
                    ApartmentNumber = o.ApartmentNumber,
                    ObjectTypeId = o.ObjectTypeId,
                    ObjectTypeName = o.ObjectType.Name,
                    TotalArea = o.TotalArea,
                    ResidentCount = o.ResidentCount,
                    NormConsumption = o.ObjectType.NormConsumption  // ← ДОБАВЛЕНО
                })
                .OrderBy(o => o.City)
                .ThenBy(o => o.Street)
                .ThenBy(o => o.HouseNumber)
                .ToListAsync(cancellationToken);
        }
    }
}