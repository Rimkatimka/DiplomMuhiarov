using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ConsumptionObjectRepository : IConsumptionObjectRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ConsumptionObjectRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<ConsumptionObjectDto> GetAll()
        {
            System.Diagnostics.Debug.WriteLine("GetAll() — прямой запрос в БД");

            var objects = _context.ConsumptionObject
                .AsNoTracking()
                .ToList();

            var result = new List<ConsumptionObjectDto>();
            foreach (var o in objects)
            {
                var street = _context.Street
                    .AsNoTracking()
                    .FirstOrDefault(s => s.Id == o.StreetId);

                var city = street != null ? _context.City
                    .AsNoTracking()
                    .FirstOrDefault(c => c.Id == street.CityId) : null;

                var region = city != null ? _context.Region
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Id == city.RegionId) : null;

                var typeName = _context.ObjectType
                    .AsNoTracking()
                    .Where(t => t.Id == o.ObjectTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? "Неизвестно";

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
            var o = _context.ConsumptionObject.FirstOrDefault(x => x.Id == id);
            if (o == null) return null;

            var street = _context.Street.FirstOrDefault(s => s.Id == o.StreetId);
            var city = street != null ? _context.City.FirstOrDefault(c => c.Id == street.CityId) : null;
            var region = city != null ? _context.Region.FirstOrDefault(r => r.Id == city.RegionId) : null;
            var typeName = _context.ObjectType.Where(t => t.Id == o.ObjectTypeId).Select(t => t.Name).FirstOrDefault() ?? "Неизвестно";

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
        }

        public void Update(ConsumptionObjectDto dto)
        {
            var entity = _context.ConsumptionObject.Find(dto.Id);
            if (entity != null)
            {
                entity.StreetId = dto.StreetId;
                entity.HouseNumber = dto.HouseNumber;
                entity.ApartmentNumber = dto.ApartmentNumber;
                entity.ObjectTypeId = dto.ObjectTypeId;
                entity.TotalArea = dto.TotalArea;
                entity.ResidentCount = dto.ResidentCount;

                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.ConsumptionObject.Find(id);
            if (entity != null)
            {
                _context.ConsumptionObject.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}