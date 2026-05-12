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
                .AsNoTracking()  // ← отключает кэш EF
                .ToList();

            var result = new List<ConsumptionObjectDto>();
            foreach (var o in objects)
            {
                var streetName = _context.Street
                    .AsNoTracking()
                    .Where(s => s.Id == o.StreetId)
                    .Select(s => s.Name)
                    .FirstOrDefault() ?? "Неизвестно";

                var typeName = _context.ObjectType
                    .AsNoTracking()
                    .Where(t => t.Id == o.ObjectTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? "Неизвестно";

                result.Add(new ConsumptionObjectDto
                {
                    Id = o.Id,
                    Street = streetName,
                    StreetId = o.StreetId,
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

            var streetName = _context.Street
                .Where(s => s.Id == o.StreetId)
                .Select(s => s.Name)
                .FirstOrDefault() ?? "Неизвестно";

            var typeName = _context.ObjectType
                .Where(t => t.Id == o.ObjectTypeId)
                .Select(t => t.Name)
                .FirstOrDefault() ?? "Неизвестно";

            return new ConsumptionObjectDto
            {
                Id = o.Id,                         
                Street = streetName,
                StreetId = o.StreetId,
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
                System.Diagnostics.Debug.WriteLine($"=== БЫЛО ===");
                System.Diagnostics.Debug.WriteLine($"StreetId: {entity.StreetId}, HouseNumber: {entity.HouseNumber}");
                System.Diagnostics.Debug.WriteLine($"ApartmentNumber: {entity.ApartmentNumber}, ObjectTypeId: {entity.ObjectTypeId}");
                System.Diagnostics.Debug.WriteLine($"TotalArea: {entity.TotalArea}, ResidentCount: {entity.ResidentCount}");

                System.Diagnostics.Debug.WriteLine($"=== СТАЛО ===");
                System.Diagnostics.Debug.WriteLine($"StreetId: {dto.StreetId}, HouseNumber: {dto.HouseNumber}");
                System.Diagnostics.Debug.WriteLine($"ApartmentNumber: {dto.ApartmentNumber}, ObjectTypeId: {dto.ObjectTypeId}");
                System.Diagnostics.Debug.WriteLine($"TotalArea: {dto.TotalArea}, ResidentCount: {dto.ResidentCount}");

                entity.StreetId = dto.StreetId;
                entity.HouseNumber = dto.HouseNumber;
                entity.ApartmentNumber = dto.ApartmentNumber;
                entity.ObjectTypeId = dto.ObjectTypeId;
                entity.TotalArea = dto.TotalArea;
                entity.ResidentCount = dto.ResidentCount;

                int result = _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SaveChanges вернул: {result}");
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