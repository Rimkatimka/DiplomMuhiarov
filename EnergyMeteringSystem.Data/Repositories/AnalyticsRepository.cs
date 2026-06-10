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

        // ✅ НОВЫЙ АСИНХРОННЫЙ МЕТОД
        public async Task<AnalyticsDataDto> GetConsumptionDataAsync(int year, int month)
        {
            var result = new AnalyticsDataDto();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Асинхронная загрузка показаний
            var readingsForMonth = await Query<MeterReading>()
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToListAsync();

            var consumptionByObject = new List<ConsumptionTemp>();

            foreach (var reading in readingsForMonth)
            {
                if (reading == null) continue;

                var prevReading = await Query<MeterReading>()
                    .Where(r => r.MeterId == reading.MeterId && r.ReadingDate < reading.ReadingDate)
                    .OrderByDescending(r => r.ReadingDate)
                    .FirstOrDefaultAsync();

                decimal consumption = prevReading != null
                    ? reading.Value - prevReading.Value
                    : reading.Value;

                if (consumption <= 0) continue;

                var meterInfo = await Query<Meter>()
                    .Where(m => m.Id == reading.MeterId)
                    .Select(m => new { m.ConsumptionObjectId })
                    .FirstOrDefaultAsync();

                if (meterInfo == null) continue;

                var objectInfo = await Query<ConsumptionObject>()
                    .Where(o => o.Id == meterInfo.ConsumptionObjectId)
                    .Select(o => new { o.Id, o.HouseNumber, o.ApartmentNumber, o.StreetId, o.ObjectTypeId })
                    .FirstOrDefaultAsync();

                if (objectInfo == null) continue;

                var street = await Query<Street>()
                    .Where(s => s.Id == objectInfo.StreetId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();

                var objectType = await Query<ObjectType>()
                    .Where(t => t.Id == objectInfo.ObjectTypeId)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync();

                consumptionByObject.Add(new ConsumptionTemp
                {
                    ObjectId = objectInfo.Id,
                    Address = (street ?? "") + ", д. " + objectInfo.HouseNumber +
                              (!string.IsNullOrEmpty(objectInfo.ApartmentNumber) ? ", кв. " + objectInfo.ApartmentNumber : ""),
                    ObjectType = objectType ?? "Неизвестно",
                    Consumption = consumption
                });
            }

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