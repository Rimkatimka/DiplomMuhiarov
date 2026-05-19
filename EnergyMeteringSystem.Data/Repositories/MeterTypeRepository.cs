using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Collections.Generic;
using System.Linq;

namespace EnergyMeteringSystem.Data.Repositories.EnergyMeteringSystem.Data.Repositories
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
            var query = from mt in _context.MeterType
                        join vi in _context.VerificationInterval on mt.Id equals vi.MeterTypeId into viJoin
                        from vi in viJoin.DefaultIfEmpty()
                        select new MeterTypeDto
                        {
                            Id = mt.Id,
                            Name = mt.Name,
                            Voltage = mt.Voltage,
                            MaxCurrent = mt.MaxCurrent,
                            AccuracyClass = mt.AccuracyClass,
                            DigitCount = mt.DigitCount,
                            DecimalPlaces = mt.DecimalPlaces,
                            ServiceLifeYears = mt.ServiceLifeYears,
                            VerificationIntervalYears = vi != null ? vi.Years : 16
                        };

            return query.ToList();
        }

        public MeterTypeDto GetById(int id)
        {
            var query = from mt in _context.MeterType
                        join vi in _context.VerificationInterval on mt.Id equals vi.MeterTypeId into viJoin
                        from vi in viJoin.DefaultIfEmpty()
                        where mt.Id == id
                        select new MeterTypeDto
                        {
                            Id = mt.Id,
                            Name = mt.Name,
                            Voltage = mt.Voltage,
                            MaxCurrent = mt.MaxCurrent,
                            AccuracyClass = mt.AccuracyClass,
                            DigitCount = mt.DigitCount,
                            DecimalPlaces = mt.DecimalPlaces,
                            ServiceLifeYears = mt.ServiceLifeYears,
                            VerificationIntervalYears = vi != null ? vi.Years : 16
                        };

            return query.FirstOrDefault();
        }
    }
}