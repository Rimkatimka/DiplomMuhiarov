using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class AnalyticsRepository : BaseRepository
    {
        public async Task<AnalyticsDataDto> GetConsumptionDataAsync(int year, int month)
        {
            System.Diagnostics.Debug.WriteLine($"GetConsumptionDataAsync: Year={year}, Month={month}");

            var result = new AnalyticsDataDto();

            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var prevMonthDate = startDate.AddMonths(-1);

                // Получаем показания за текущий и предыдущий месяц
                var readings = await _context.MeterReading
                    .Include(r => r.Meter)
                    .Include(r => r.Meter.ConsumptionObject)
                    .Include(r => r.Meter.ConsumptionObject.Street)
                    .Include(r => r.Meter.ConsumptionObject.Street.City)
                    .Include(r => r.Meter.ConsumptionObject.Street.City.Region)
                    .Include(r => r.Meter.ConsumptionObject.ObjectType)
                    .Where(r => r.ReadingDate >= prevMonthDate && r.ReadingDate <= endDate)
                    .OrderBy(r => r.MeterId)
                    .ThenBy(r => r.ReadingDate)
                    .ToListAsync();

                if (!readings.Any()) return result;

                var consumptionByObject = new Dictionary<int, (string Address, string ObjectType, decimal Consumption)>();

                var groupedByMeter = readings.GroupBy(r => r.MeterId);

                foreach (var meterGroup in groupedByMeter)
                {
                    var orderedReadings = meterGroup.OrderBy(r => r.ReadingDate).ToList();
                    var meter = meterGroup.First().Meter;

                    if (meter?.ConsumptionObject == null) continue;

                    var obj = meter.ConsumptionObject;
                    var street = obj.Street;
                    var city = street?.City;
                    var region = city?.Region;
                    var objectType = obj.ObjectType;

                    string fullAddress = $"{region?.Name}, {city?.Name}, {street?.Name}, {obj.HouseNumber}";
                    if (!string.IsNullOrEmpty(obj.ApartmentNumber))
                        fullAddress += $"/{obj.ApartmentNumber}";

                    // Находим показания за текущий месяц
                    var currentReadings = orderedReadings
                        .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                        .ToList();

                    if (!currentReadings.Any()) continue;

                    var currentValue = currentReadings.Last().Value;

                    // Находим показания за предыдущий месяц (или начальное)
                    decimal prevValue = meter.InitialReading;
                    var prevReadings = orderedReadings
                        .Where(r => r.ReadingDate < startDate)
                        .ToList();

                    if (prevReadings.Any())
                    {
                        prevValue = prevReadings.Last().Value;
                    }

                    decimal consumption = currentValue - prevValue;
                    if (consumption <= 0) continue;

                    if (!consumptionByObject.ContainsKey(obj.Id))
                    {
                        consumptionByObject[obj.Id] = (fullAddress, objectType?.Name ?? "Неизвестно", 0);
                    }

                    var existing = consumptionByObject[obj.Id];
                    consumptionByObject[obj.Id] = (existing.Address, existing.ObjectType, existing.Consumption + consumption);
                }

                // Формируем результат
                var objectList = consumptionByObject.Select(x => new
                {
                    x.Key,
                    x.Value.Address,
                    x.Value.ObjectType,
                    x.Value.Consumption
                }).ToList();

                result.TotalConsumption = objectList.Sum(x => x.Consumption);
                result.MaxConsumption = objectList.Any() ? objectList.Max(x => x.Consumption) : 0;
                result.AverageConsumption = objectList.Any() ? result.TotalConsumption / objectList.Count : 0;

                // ТОП объекты
                result.TopObjects = objectList
                    .OrderByDescending(x => x.Consumption)
                    .Take(15)
                    .Select((x, index) => new TopObjectDto
                    {
                        Rank = index + 1,
                        ObjectId = x.Key,
                        Address = x.Address,
                        ObjectType = x.ObjectType,
                        Consumption = x.Consumption,
                        Percentage = result.TotalConsumption > 0 ? (x.Consumption / result.TotalConsumption) * 100 : 0
                    })
                    .ToList();

                // Распределение по типам
                result.TypeDistribution = objectList
                    .GroupBy(x => x.ObjectType)
                    .Select(g => new TypeDistributionDto
                    {
                        TypeName = g.Key,
                        Consumption = g.Sum(x => x.Consumption),
                        Percentage = result.TotalConsumption > 0 ? (g.Sum(x => x.Consumption) / result.TotalConsumption) * 100 : 0
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Итог: Объектов={result.TopObjects.Count}, Типов={result.TypeDistribution.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetConsumptionDataAsync ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

            return result;
        }
    }
}