using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ConsumptionObjectRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ConsumptionObjectRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<ConsumptionObjectDto> GetAll()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ConsumptionObjectRepository.GetAll() начат");

                var objects = _context.ConsumptionObject
                    .Include("Street")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Загружено {objects.Count} объектов из БД");

                var result = objects.Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    Street = o.Street?.Name ?? "Неизвестно",
                    HouseNumber = o.HouseNumber,
                    ApartmentNumber = o.ApartmentNumber,
                    ObjectTypeName = o.ObjectType?.Name ?? "Неизвестно",
                    TotalArea = o.TotalArea,
                    ResidentCount = o.ResidentCount
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Возвращаем {result.Count} объектов");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetAll: {ex.Message}");
                return new List<ConsumptionObjectDto>();
            }
        }

        public ConsumptionObjectDto GetById(int id)
        {
            var o = _context.ConsumptionObject
                .Include("ObjectType")
                .Include("Street")
                .FirstOrDefault(x => x.Id == id);

            if (o == null) return null;

            return new ConsumptionObjectDto
            {
                Id = o.Id,
                Name = o.Name,
                Street = o.Street.Name,
                HouseNumber = o.HouseNumber,
                ApartmentNumber = o.ApartmentNumber,
                ObjectTypeName = o.ObjectType?.Name,
                TotalArea = o.TotalArea,
                ResidentCount = o.ResidentCount
            };
        }

        // ✅ Добавьте этот метод
        public void Add(ConsumptionObjectDto dto)
        {
            var entity = new ConsumptionObject
            {
                Name = dto.Street + ", " + dto.HouseNumber,
                ObjectTypeId = dto.ObjectTypeId,
                StreetId = dto.StreetId,
                HouseNumber = dto.HouseNumber,
                ApartmentNumber = dto.ApartmentNumber,
                TotalArea = dto.TotalArea,
                ResidentCount = dto.ResidentCount
            };

            _context.ConsumptionObject.Add(entity);
            _context.SaveChanges();
        }

        // ✅ Добавьте метод Update
        public void Update(ConsumptionObjectDto dto)
        {
            var entity = _context.ConsumptionObject.Find(dto.Id);
            if (entity != null)
            {
                entity.Name = dto.Street + ", " + dto.HouseNumber;
                entity.ObjectTypeId = dto.ObjectTypeId;
                entity.StreetId = dto.StreetId;
                entity.HouseNumber = dto.HouseNumber;
                entity.ApartmentNumber = dto.ApartmentNumber;
                entity.TotalArea = dto.TotalArea;
                entity.ResidentCount = dto.ResidentCount;

                _context.SaveChanges();
            }
        }

        // ✅ Добавьте метод Delete
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