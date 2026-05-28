using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

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
                .Select(mt => new MeterTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    Voltage = mt.Voltage,
                    MaxCurrent = mt.MaxCurrent,
                    AccuracyClass = mt.AccuracyClass,
                    DigitCount = mt.DigitCount,
                    DecimalPlaces = mt.DecimalPlaces,
                    ServiceLifeYears = mt.ServiceLifeYears,
                    VerificationIntervalYears = GetVerificationInterval(mt.Id)
                })
                .ToList();
        }

        public MeterTypeDto GetById(int id)
        {
            var mt = _context.MeterType.Find(id);
            if (mt == null) return null;

            return new MeterTypeDto
            {
                Id = mt.Id,
                Name = mt.Name,
                Voltage = mt.Voltage,
                MaxCurrent = mt.MaxCurrent,
                AccuracyClass = mt.AccuracyClass,
                DigitCount = mt.DigitCount,
                DecimalPlaces = mt.DecimalPlaces,
                ServiceLifeYears = mt.ServiceLifeYears,
                VerificationIntervalYears = GetVerificationInterval(mt.Id)
            };
        }

        private int? GetVerificationInterval(int meterTypeId)
        {
            var interval = _context.VerificationInterval
                .FirstOrDefault(vi => vi.MeterTypeId == meterTypeId);
            return interval?.Years;
        }
    }
}