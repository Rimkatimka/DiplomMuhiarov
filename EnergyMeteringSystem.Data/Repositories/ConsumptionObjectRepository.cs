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
                        ResidentCount = o.ResidentCount
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
                    ResidentCount = o.ResidentCount
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public ConsumptionObjectDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
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

            AuditLogger.Log("INSERT", "ConsumptionObject", entity.Id, null,
                new { dto.HouseNumber, dto.ApartmentNumber, dto.ObjectTypeId });

            return entity.Id;
        }

        public void Add(ConsumptionObjectDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            bool hasMeters = await Query<Meter>().AnyAsync(m => m.ConsumptionObjectId == id, cancellationToken);
            if (hasMeters)
            {
                throw new System.InvalidOperationException("Нельзя удалить объект, у которого есть счетчики");
            }

            var oldValues = new { entity.HouseNumber, entity.ApartmentNumber };

            _context.ConsumptionObject.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("DELETE", "ConsumptionObject", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<bool> UpdateAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new
            {
                entity.HouseNumber,
                entity.ApartmentNumber,
                entity.TotalArea,
                entity.ResidentCount,
                entity.StreetId,
                entity.ObjectTypeId
            };

            var newValues = new
            {
                dto.HouseNumber,
                dto.ApartmentNumber,
                dto.TotalArea,
                dto.ResidentCount,
                dto.StreetId,
                dto.ObjectTypeId
            };

            entity.StreetId = dto.StreetId;
            entity.HouseNumber = dto.HouseNumber?.Trim();
            entity.ApartmentNumber = dto.ApartmentNumber?.Trim();
            entity.ObjectTypeId = dto.ObjectTypeId;
            entity.TotalArea = dto.TotalArea;
            entity.ResidentCount = dto.ResidentCount;

            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "ConsumptionObject", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(ConsumptionObjectDto dto)
        {
            UpdateAsync(dto).Wait();
        }
    }
}