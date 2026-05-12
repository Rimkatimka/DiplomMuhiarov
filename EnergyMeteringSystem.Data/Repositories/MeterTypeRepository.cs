using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    namespace EnergyMeteringSystem.Data.Repositories
    {
        public class MeterTypeRepository : IMeterTypeRepository
        {
            private readonly EnergyMeteringSystemEntities _context;

            public MeterTypeRepository()
            {
                _context = new EnergyMeteringSystemEntities();
            }

            public List<MeterTypeDto> GetAll()
            {
                return _context.MeterType
                    .Select(m => new MeterTypeDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        ServiceLifeYears = m.ServiceLifeYears,
                        Voltage = m.Voltage,
                        MaxCurrent = m.MaxCurrent,
                        AccuracyClass = m.AccuracyClass,
                        DigitCount = m.DigitCount,
                        DecimalPlaces = m.DecimalPlaces
                    })
                    .ToList();
            }

            public MeterTypeDto GetById(int id)
            {
                var m = _context.MeterType.Find(id);
                if (m == null) return null;

                return new MeterTypeDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    ServiceLifeYears = m.ServiceLifeYears,
                    Voltage = m.Voltage,
                    MaxCurrent = m.MaxCurrent,
                    AccuracyClass = m.AccuracyClass,
                    DigitCount = m.DigitCount,
                    DecimalPlaces = m.DecimalPlaces
                };
            }
        }
    }
}