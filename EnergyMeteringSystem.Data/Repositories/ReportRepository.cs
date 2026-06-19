using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ReportRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ReportRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        // ✅ ОСНОВНОЙ МЕТОД ДЛЯ ОТЧЕТОВ
        public async Task<List<ConsumptionReportDto>> GetConsumptionReportOptimizedAsync(DateTime startDate, DateTime endDate)
        {
            System.Diagnostics.Debug.WriteLine($"GetConsumptionReportOptimizedAsync: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");

            try
            {
                // Получаем все показания за период
                var readings = await _context.MeterReading
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .OrderBy(r => r.MeterId)
                    .ThenBy(r => r.ReadingDate)
                    .ToListAsync();

                if (!readings.Any())
                    return new List<ConsumptionReportDto>();

                var result = new List<ConsumptionReportDto>();

                // Группируем по счетчикам
                var groupedByMeter = readings.GroupBy(r => r.MeterId);

                foreach (var meterGroup in groupedByMeter)
                {
                    var meterId = meterGroup.Key;
                    var orderedReadings = meterGroup.OrderBy(r => r.ReadingDate).ToList();

                    var meter = await _context.Meter
                        .Include(m => m.ConsumptionObject)
                        .Include(m => m.ConsumptionObject.Street)
                        .Include(m => m.ConsumptionObject.Street.City)
                        .Include(m => m.ConsumptionObject.Street.City.Region)
                        .Include(m => m.ConsumptionObject.ObjectType)
                        .FirstOrDefaultAsync(m => m.Id == meterId);

                    if (meter?.ConsumptionObject == null) continue;

                    var obj = meter.ConsumptionObject;
                    var street = obj.Street;
                    var city = street?.City;
                    var region = city?.Region;
                    var objectType = obj.ObjectType;

                    string fullAddress = $"{region?.Name}, {city?.Name}, {street?.Name}, {obj.HouseNumber}";
                    if (!string.IsNullOrEmpty(obj.ApartmentNumber))
                        fullAddress += $"/{obj.ApartmentNumber}";

                    // ✅ Если только одно показание за период - берем InitialReading как начало
                    if (orderedReadings.Count == 1)
                    {
                        var reading = orderedReadings.First();
                        decimal consumption = reading.Value - meter.InitialReading;

                        if (consumption <= 0) continue;

                        result.Add(new ConsumptionReportDto
                        {
                            ObjectId = obj.Id,
                            Address = fullAddress,
                            MeterSerial = meter.SerialNumber,
                            StartDate = meter.InstallationDate,
                            EndDate = reading.ReadingDate,
                            StartValue = meter.InitialReading,
                            EndValue = reading.Value,
                            Consumption = consumption,
                            ObjectType = objectType?.Name ?? "Не указан"
                        });
                    }
                    else
                    {
                        // ✅ Несколько показаний - считаем ПОСЛЕДОВАТЕЛЬНУЮ разницу
                        for (int i = 0; i < orderedReadings.Count; i++)
                        {
                            var current = orderedReadings[i];
                            decimal startValue = (i == 0) ? meter.InitialReading : orderedReadings[i - 1].Value;
                            decimal consumption = current.Value - startValue;

                            if (consumption <= 0) continue;

                            result.Add(new ConsumptionReportDto
                            {
                                ObjectId = obj.Id,
                                Address = fullAddress,
                                MeterSerial = meter.SerialNumber,
                                StartDate = (i == 0) ? meter.InstallationDate : orderedReadings[i - 1].ReadingDate,
                                EndDate = current.ReadingDate,
                                StartValue = startValue,
                                EndValue = current.Value,
                                Consumption = consumption,
                                ObjectType = objectType?.Name ?? "Не указан"
                            });
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"  Итоговых записей: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: {ex.Message}");
                return new List<ConsumptionReportDto>();
            }
        }

        // ✅ СТАРЫЙ МЕТОД ДЛЯ СОВМЕСТИМОСТИ
        public List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate)
        {
            return GetConsumptionReportOptimizedAsync(startDate, endDate).Result;
        }

        // ✅ МЕТОД ДЛЯ ДИНАМИКИ ПО МЕСЯЦАМ
        // EnergyMeteringSystem.Data/Repositories/ReportRepository.cs

        public async Task<List<MonthlyConsumptionDto>> GetMonthlyConsumptionAsync(int year, int? month = null)
        {
            try
            {
                // ✅ Начинаем с декабря прошлого года для правильного расчета января
                var startDate = new DateTime(year - 1, 12, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var readings = await _context.MeterReading
                    .Include(r => r.Meter)
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .OrderBy(r => r.MeterId)
                    .ThenBy(r => r.ReadingDate)
                    .ToListAsync();

                // Группируем по счетчикам и считаем потребление
                var monthlyConsumption = new Dictionary<int, decimal>();

                var groupedByMeter = readings.GroupBy(r => r.MeterId);

                foreach (var meterGroup in groupedByMeter)
                {
                    var orderedReadings = meterGroup.OrderBy(r => r.ReadingDate).ToList();

                    // Получаем начальное показание счетчика
                    var meter = meterGroup.First().Meter;
                    decimal previousValue = meter?.InitialReading ?? 0;

                    foreach (var reading in orderedReadings)
                    {
                        decimal consumption = reading.Value - previousValue;
                        if (consumption > 0 && reading.ReadingDate.Year == year)
                        {
                            int monthKey = reading.ReadingDate.Month;
                            if (!monthlyConsumption.ContainsKey(monthKey))
                                monthlyConsumption[monthKey] = 0;
                            monthlyConsumption[monthKey] += consumption;
                        }
                        previousValue = reading.Value;
                    }
                }

                var result = new List<MonthlyConsumptionDto>();
                for (int m = 1; m <= 12; m++)
                {
                    result.Add(new MonthlyConsumptionDto
                    {
                        Year = year,
                        Month = m,
                        TotalConsumption = monthlyConsumption.ContainsKey(m) ? monthlyConsumption[m] : 0
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMonthlyConsumptionAsync ERROR: {ex.Message}");
                return new List<MonthlyConsumptionDto>();
            }
        }
        public async Task<List<RawReadingDto>> GetRawReadingsForPeriodAsync(DateTime from, DateTime to)
        {
            var readings = await _context.MeterReading
                .Where(r => r.ReadingDate >= from && r.ReadingDate <= to)
                .Select(r => new RawReadingDto
                {
                    MeterId = r.MeterId,
                    ReadingDate = r.ReadingDate,
                    Value = r.Value
                })
                .ToListAsync();

            return readings;
        }
    }

    // ✅ ВСПОМОГАТЕЛЬНЫЙ DTO ДЛЯ ДИНАМИКИ
    public class MonthlyConsumptionDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalConsumption { get; set; }
    }
    public class RawReadingDto
    {
        public int MeterId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
    }
}