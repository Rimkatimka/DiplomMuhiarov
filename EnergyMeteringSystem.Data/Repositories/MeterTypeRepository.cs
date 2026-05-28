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
            // 1. Сначала получаем все типы счетчиков
            var meterTypes = _context.MeterType.ToList();

            // 2. Получаем все интервалы поверки
            var intervals = _context.VerificationInterval.ToDictionary(vi => vi.MeterTypeId, vi => vi.Years);

            // 3. Формируем результат в памяти
            var result = new List<MeterTypeDto>();
            foreach (var mt in meterTypes)
            {
                result.Add(new MeterTypeDto
                {
                    Id = mt.Id,
                    Name = mt.Name,
                    Voltage = mt.Voltage,
                    MaxCurrent = mt.MaxCurrent,
                    AccuracyClass = mt.AccuracyClass,
                    DigitCount = mt.DigitCount,
                    DecimalPlaces = mt.DecimalPlaces,
                    ServiceLifeYears = mt.ServiceLifeYears,
                    VerificationIntervalYears = intervals.ContainsKey(mt.Id) ? intervals[mt.Id] : (int?)null
                });
            }

            return result;
        }

        public MeterTypeDto GetById(int id)
        {
            var mt = _context.MeterType.FirstOrDefault(m => m.Id == id);
            if (mt == null) return null;

            var interval = _context.VerificationInterval
                .FirstOrDefault(vi => vi.MeterTypeId == id);

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
                VerificationIntervalYears = interval?.Years
            };
        }
    }
}