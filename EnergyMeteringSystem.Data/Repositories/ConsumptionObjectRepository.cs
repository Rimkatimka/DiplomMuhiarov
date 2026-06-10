using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ConsumptionObjectRepository : BaseRepository, IConsumptionObjectRepository
    {
        // Синхронный (для совместимости)
        public List<ConsumptionObjectDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ АСИНХРОННЫЙ
        public async Task<List<ConsumptionObjectDto>> GetAllAsync()
        {
            System.Diagnostics.Debug.WriteLine("GetAllAsync() — асинхронный запрос в БД");

            var objects = await Query<ConsumptionObject>().ToListAsync();

            var result = new List<ConsumptionObjectDto>();
            foreach (var o in objects)
            {
                var street = await Query<Street>()
                    .FirstOrDefaultAsync(s => s.Id == o.StreetId);

                var city = street != null ? await Query<City>()
                    .FirstOrDefaultAsync(c => c.Id == street.CityId) : null;

                var region = city != null ? await Query<Region>()
                    .FirstOrDefaultAsync(r => r.Id == city.RegionId) : null;

                var typeName = await Query<ObjectType>()
                    .Where(t => t.Id == o.ObjectTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync() ?? "Неизвестно";

                result.Add(new ConsumptionObjectDto
                {
                    Id = o.Id,
                    Street = street?.Name ?? "Неизвестно",
                    StreetId = o.StreetId,
                    City = city?.Name ?? "Неизвестно",
                    CityId = city?.Id ?? 0,
                    Region = region?.Name ?? "Неизвестно",
                    RegionId = region?.Id ?? 0,
                    HouseNumber = o.HouseNumber,
                    ApartmentNumber = o.ApartmentNumber,
                    ObjectTypeId = o.ObjectTypeId,
                    ObjectTypeName = typeName,
                    TotalArea = o.TotalArea,
                    ResidentCount = o.ResidentCount
                });
            }

            return result;
        }

        public ConsumptionObjectDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<ConsumptionObjectDto> GetByIdAsync(int id)
        {
            var o = await Query<ConsumptionObject>().FirstOrDefaultAsync(x => x.Id == id);
            if (o == null) return null;

            var street = await Query<Street>().FirstOrDefaultAsync(s => s.Id == o.StreetId);
            var city = street != null ? await Query<City>().FirstOrDefaultAsync(c => c.Id == street.CityId) : null;
            var region = city != null ? await Query<Region>().FirstOrDefaultAsync(r => r.Id == city.RegionId) : null;
            var typeName = await Query<ObjectType>().Where(t => t.Id == o.ObjectTypeId).Select(t => t.Name).FirstOrDefaultAsync() ?? "Неизвестно";

            return new ConsumptionObjectDto
            {
                Id = o.Id,
                Street = street?.Name ?? "Неизвестно",
                StreetId = o.StreetId,
                City = city?.Name ?? "Неизвестно",
                CityId = city?.Id ?? 0,
                Region = region?.Name ?? "Неизвестно",
                RegionId = region?.Id ?? 0,
                HouseNumber = o.HouseNumber,
                ApartmentNumber = o.ApartmentNumber,
                ObjectTypeId = o.ObjectTypeId,
                ObjectTypeName = typeName,
                TotalArea = o.TotalArea,
                ResidentCount = o.ResidentCount
            };
        }

        public void Add(ConsumptionObjectDto dto)
        {
            var entity = new ConsumptionObject
            {
                StreetId = dto.StreetId,
                HouseNumber = dto.HouseNumber,
                ApartmentNumber = dto.ApartmentNumber,
                ObjectTypeId = dto.ObjectTypeId,
                TotalArea = dto.TotalArea,
                ResidentCount = dto.ResidentCount
            };

            _context.ConsumptionObject.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "ConsumptionObject", entity.Id, null,
                new { dto.HouseNumber, dto.ApartmentNumber, dto.ObjectTypeId });
        }

        public void Delete(int id)
        {
            var entity = _context.ConsumptionObject.Find(id);
            if (entity != null)
            {
                var oldValues = new { entity.HouseNumber, entity.ApartmentNumber };

                _context.ConsumptionObject.Remove(entity);
                _context.SaveChanges();

                AuditLogger.Log("DELETE", "ConsumptionObject", id, oldValues, null);
            }
        }

        public void Update(ConsumptionObjectDto dto)
        {
            var entity = _context.ConsumptionObject.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.HouseNumber, entity.ApartmentNumber, entity.TotalArea, entity.ResidentCount };
                var newValues = new { dto.HouseNumber, dto.ApartmentNumber, dto.TotalArea, dto.ResidentCount };

                entity.StreetId = dto.StreetId;
                entity.HouseNumber = dto.HouseNumber;
                entity.ApartmentNumber = dto.ApartmentNumber;
                entity.ObjectTypeId = dto.ObjectTypeId;
                entity.TotalArea = dto.TotalArea;
                entity.ResidentCount = dto.ResidentCount;

                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "ConsumptionObject", entity.Id, oldValues, newValues);
            }
        }
    }
}