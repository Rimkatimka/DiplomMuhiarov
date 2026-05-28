using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class AnalyticsRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public AnalyticsRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public AnalyticsDataDto GetConsumptionData(int year, int month)
        {
            var result = new AnalyticsDataDto();

            var consumptionByObject = _context.Accrual
                .Where(a => a.PeriodYear == year && a.PeriodMonth == month)
                .Join(_context.ConsumptionObject,
                    a => a.ConsumptionObjectId,
                    o => o.Id,
                    (a, o) => new
                    {
                        o.Id,
                        Address = o.Street.Name + ", д. " + o.HouseNumber +
                                 (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                        ObjectType = o.ObjectType.Name,
                        Consumption = a.ConsumptionValue
                    })
                .ToList();

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
                    ObjectId = x.Id,
                    Address = x.Address,
                    ObjectType = x.ObjectType,
                    Consumption = x.Consumption,
                    Percentage = result.TotalConsumption > 0
                        ? x.Consumption / result.TotalConsumption * 100
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
                        ? g.Sum(x => x.Consumption) / result.TotalConsumption * 100
                        : 0
                })
                .ToList();

            return result;
        }
    }
}