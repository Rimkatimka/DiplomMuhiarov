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
                // ✅ ПРОСТОЙ ЗАПРОС - ПОЛУЧАЕМ ТОЛЬКО ПОКАЗАНИЯ
                var readings = await _context.MeterReading
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .OrderBy(r => r.MeterId)
                    .ThenBy(r => r.ReadingDate)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"  Найдено показаний: {readings.Count}");

                if (!readings.Any())
                {
                    System.Diagnostics.Debug.WriteLine("  Нет показаний за период");
                    return new List<ConsumptionReportDto>();
                }

                var result = new List<ConsumptionReportDto>();

                // Группируем по счетчикам
                var groupedByMeter = readings.GroupBy(r => r.MeterId);

                foreach (var meterGroup in groupedByMeter)
                {
                    var meterId = meterGroup.Key;
                    var orderedReadings = meterGroup.OrderBy(r => r.ReadingDate).ToList();

                    // Получаем информацию о счетчике и объекте
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

                    // Если только одно показание - используем InitialReading
                    if (orderedReadings.Count == 1)
                    {
                        var reading = orderedReadings.First();
                        decimal consumption = reading.Value - meter.InitialReading;

                        if (consumption <= 0) continue;

                        string fullAddress = $"{region?.Name}, {city?.Name}, {street?.Name}, {obj.HouseNumber}";
                        if (!string.IsNullOrEmpty(obj.ApartmentNumber))
                            fullAddress += $"/{obj.ApartmentNumber}";

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
                        // Несколько показаний - берем первое и последнее
                        var first = orderedReadings.First();
                        var last = orderedReadings.Last();
                        decimal consumption = last.Value - first.Value;

                        if (consumption <= 0) continue;

                        string fullAddress = $"{region?.Name}, {city?.Name}, {street?.Name}, {obj.HouseNumber}";
                        if (!string.IsNullOrEmpty(obj.ApartmentNumber))
                            fullAddress += $"/{obj.ApartmentNumber}";

                        result.Add(new ConsumptionReportDto
                        {
                            ObjectId = obj.Id,
                            Address = fullAddress,
                            MeterSerial = meter.SerialNumber,
                            StartDate = first.ReadingDate,
                            EndDate = last.ReadingDate,
                            StartValue = first.Value,
                            EndValue = last.Value,
                            Consumption = consumption,
                            ObjectType = objectType?.Name ?? "Не указан"
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine($"  Итоговых записей: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return new List<ConsumptionReportDto>();
            }
        }

        // ✅ СТАРЫЙ МЕТОД ДЛЯ СОВМЕСТИМОСТИ
        public List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate)
        {
            return GetConsumptionReportOptimizedAsync(startDate, endDate).Result;
        }

        // ✅ МЕТОД ДЛЯ ДИНАМИКИ ПО МЕСЯЦАМ
        public async Task<List<MonthlyConsumptionDto>> GetMonthlyConsumptionAsync(int year, int? month = null)
        {
            try
            {
                var query = _context.MeterReading
                    .Include(r => r.Meter)
                    .Include(r => r.Meter.ConsumptionObject)
                    .Where(r => r.ReadingDate.Year == year);

                if (month.HasValue)
                {
                    query = query.Where(r => r.ReadingDate.Month == month.Value);
                }

                var readings = await query
                    .GroupBy(r => new { r.ReadingDate.Year, r.ReadingDate.Month })
                    .Select(g => new MonthlyConsumptionDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalConsumption = g.Sum(r => r.Value)
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                return readings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMonthlyConsumptionAsync ERROR: {ex.Message}");
                return new List<MonthlyConsumptionDto>();
            }
        }
    }

    // ✅ ВСПОМОГАТЕЛЬНЫЙ DTO ДЛЯ ДИНАМИКИ
    public class MonthlyConsumptionDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalConsumption { get; set; }
    }
}