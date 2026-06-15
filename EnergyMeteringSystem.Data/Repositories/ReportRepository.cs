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
            try
            {
                var result = new List<ConsumptionReportDto>();

                // Получаем все показания за период
                var readings = await _context.MeterReading
                    .Include(r => r.Meter)
                    .Include(r => r.Meter.ConsumptionObject)
                    .Include(r => r.Meter.ConsumptionObject.Street)
                    .Include(r => r.Meter.ConsumptionObject.Street.City)
                    .Include(r => r.Meter.ConsumptionObject.Street.City.Region)
                    .Include(r => r.Meter.ConsumptionObject.ObjectType)
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .OrderBy(r => r.MeterId)
                    .ThenBy(r => r.ReadingDate)
                    .ToListAsync();

                // Группируем по счетчикам
                var groupedByMeter = readings.GroupBy(r => r.MeterId);

                foreach (var meterGroup in groupedByMeter)
                {
                    var meter = meterGroup.First().Meter;
                    if (meter?.ConsumptionObject == null) continue;

                    var obj = meter.ConsumptionObject;
                    var street = obj.Street;
                    var city = street?.City;
                    var region = city?.Region;
                    var objectType = obj.ObjectType;

                    var orderedReadings = meterGroup.OrderBy(r => r.ReadingDate).ToList();
                    if (orderedReadings.Count < 2) continue;

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

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetConsumptionReportOptimizedAsync ERROR: {ex.Message}");
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