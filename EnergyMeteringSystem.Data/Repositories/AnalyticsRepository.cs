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
        // Существующий синхронный метод (оставляем для совместимости)
        public AnalyticsDataDto GetConsumptionData(int year, int month)
        {
            return GetConsumptionDataAsync(year, month).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ АСИНХРОННЫЙ МЕТОД
        public async Task<AnalyticsDataDto> GetConsumptionDataAsync(int year, int month)
        {
            var result = new AnalyticsDataDto();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // ========== 1. Получаем все показания за месяц одним запросом ==========
            var readingsForMonth = await Query<MeterReading>()
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToListAsync();

            if (!readingsForMonth.Any())
                return result;

            // ========== 2. Получаем ВСЕ предыдущие показания одним запросом ==========
            var meterIds = readingsForMonth.Select(r => r.MeterId).Distinct().ToList();

            var previousReadings = await Query<MeterReading>()
                .Where(r => meterIds.Contains(r.MeterId) && readingsForMonth.Any(rm => rm.ReadingDate > r.ReadingDate))
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToDictionaryAsync(r => r.MeterId, r => r);

            // ========== 3. Получаем информацию о счетчиках одним запросом ==========
            var meterInfos = await Query<Meter>()
                .Where(m => meterIds.Contains(m.Id))
                .Select(m => new { m.Id, m.ConsumptionObjectId })
                .ToDictionaryAsync(m => m.Id, m => m.ConsumptionObjectId);

            // ========== 4. Получаем информацию об объектах одним запросом ==========
            var objectIds = meterInfos.Values.Distinct().ToList();

            var objectInfos = await Query<ConsumptionObject>()
                .Where(o => objectIds.Contains(o.Id))
                .Select(o => new {
                    o.Id,
                    o.HouseNumber,
                    o.ApartmentNumber,
                    o.StreetId,
                    o.ObjectTypeId,
                    StreetName = o.Street.Name,
                    ObjectTypeName = o.ObjectType.Name
                })
                .ToDictionaryAsync(o => o.Id, o => o);

            // ========== 5. Формируем результат без дополнительных запросов ==========
            var consumptionByObject = new List<ConsumptionTemp>();

            foreach (var reading in readingsForMonth)
            {
                if (reading == null) continue;

                // Получаем предыдущее показание из словаря
                previousReadings.TryGetValue(reading.MeterId, out var prevReading);

                decimal consumption = prevReading != null
                    ? reading.Value - prevReading.Value
                    : reading.Value;

                if (consumption <= 0) continue;

                // Получаем ID объекта из словаря
                if (!meterInfos.TryGetValue(reading.MeterId, out var objectId))
                    continue;

                // Получаем информацию об объекте из словаря
                if (!objectInfos.TryGetValue(objectId, out var objectInfo))
                    continue;

                consumptionByObject.Add(new ConsumptionTemp
                {
                    ObjectId = objectInfo.Id,
                    Address = (objectInfo.StreetName ?? "") + ", д. " + objectInfo.HouseNumber +
                              (!string.IsNullOrEmpty(objectInfo.ApartmentNumber) ? ", кв. " + objectInfo.ApartmentNumber : ""),
                    ObjectType = objectInfo.ObjectTypeName ?? "Неизвестно",
                    Consumption = consumption
                });
            }

            // ========== 6. Формируем результат ==========
            result.TotalConsumption = consumptionByObject.Sum(x => x.Consumption);
            result.MaxConsumption = consumptionByObject.Any()
                ? consumptionByObject.Max(x => x.Consumption)
                : 0;
            result.AverageConsumption = consumptionByObject.Any()
                ? result.TotalConsumption / consumptionByObject.Count
                : 0;

            result.TopObjects = consumptionByObject
                .OrderByDescending(x => x.Consumption)
                .Take(15)
                .Select((x, index) => new TopObjectDto
                {
                    Rank = index + 1,
                    ObjectId = x.ObjectId,
                    Address = x.Address,
                    ObjectType = x.ObjectType,
                    Consumption = x.Consumption,
                    Percentage = result.TotalConsumption > 0
                        ? (x.Consumption / result.TotalConsumption) * 100
                        : 0
                })
                .ToList();

            result.TypeDistribution = consumptionByObject
                .GroupBy(x => x.ObjectType)
                .Select(g => new TypeDistributionDto
                {
                    TypeName = g.Key,
                    Consumption = g.Sum(x => x.Consumption),
                    Percentage = result.TotalConsumption > 0
                        ? (g.Sum(x => x.Consumption) / result.TotalConsumption) * 100
                        : 0
                })
                .ToList();

            return result;
        }

        private class ConsumptionTemp
        {
            public int ObjectId { get; set; }
            public string Address { get; set; }
            public string ObjectType { get; set; }
            public decimal Consumption { get; set; }
        }
    }
}