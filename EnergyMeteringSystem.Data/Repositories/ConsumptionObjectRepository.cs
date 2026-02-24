using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // ✅ Используем строки вместо лямбд
            var objects = _context.ConsumptionObject
                .Include("ObjectType")
                .Include("Street")
                .ToList();

            return objects.Select(o => new ConsumptionObjectDto
            {
                Id = o.Id,
                Name = o.Name,
                Address = o.Street.Name + ", д. " + o.HouseNumber +
                          (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                ObjectTypeName = o.ObjectType?.Name,
                TotalArea = o.TotalArea,
                ResidentCount = o.ResidentCount
            }).ToList();
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
                Address = o.Street.Name + ", д. " + o.HouseNumber +
                          (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                ObjectTypeName = o.ObjectType?.Name,
                TotalArea = o.TotalArea,
                ResidentCount = o.ResidentCount
            };
        }
    }
}
