using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterTypeRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterTypeRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<MeterTypeDto> GetAll()
        {
            try
            {
                var types = _context.MeterType.ToList();

                return types.Select(t => new MeterTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Voltage = t.Voltage,
                    MaxCurrent = t.MaxCurrent,
                    AccuracyClass = t.AccuracyClass,
                    DigitCount = t.DigitCount,
                    DecimalPlaces = t.DecimalPlaces
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в MeterTypeRepository.GetAll: {ex.Message}");
                return new List<MeterTypeDto>();
            }
        }

        public MeterTypeDto GetById(int id)
        {
            try
            {
                var t = _context.MeterType.Find(id);
                if (t == null) return null;

                return new MeterTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Voltage = t.Voltage,
                    MaxCurrent = t.MaxCurrent,
                    AccuracyClass = t.AccuracyClass,
                    DigitCount = t.DigitCount,
                    DecimalPlaces = t.DecimalPlaces
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в MeterTypeRepository.GetById: {ex.Message}");
                return null;
            }
        }
    }
}