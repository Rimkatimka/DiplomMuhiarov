using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class HierarchyAnalyticsRepository : BaseRepository
    {
        // Синхронные методы (оставляем для совместимости)
        public List<RegionAnalyticsDto> GetAnalyticsByRegion(int year, int month)
        {
            return GetAnalyticsByRegionAsync(year, month).Result;
        }

        public RegionAnalyticsDto GetAnalyticsByRegionId(int regionId, int year, int month)
        {
            return GetAnalyticsByRegionIdAsync(regionId, year, month).Result;
        }

        public List<ObjectAnalyticsDto> GetTopObjectsByRegion(int regionId, int year, int month, int topCount = 10)
        {
            return GetTopObjectsByRegionAsync(regionId, year, month, topCount).Result;
        }

        // ✅ АСИНХРОННЫЕ МЕТОДЫ
        public async Task<List<RegionAnalyticsDto>> GetAnalyticsByRegionAsync(int year, int month)
        {
            var result = new List<RegionAnalyticsDto>();

            var consumptionByObject = await GetConsumptionByObjectAsync(year, month);
            var regions = await Query<Region>().ToListAsync();

            foreach (var region in regions)
            {
                var regionData = new RegionAnalyticsDto
                {
                    RegionId = region.Id,
                    RegionName = region.Name,
                    Cities = new List<CityAnalyticsDto>()
                };

                var objectsInRegion = consumptionByObject
                    .Where(o => o.RegionId == region.Id)
                    .ToList();

                if (!objectsInRegion.Any()) continue;

                var citiesGroup = objectsInRegion.GroupBy(o => new { o.CityId, o.CityName });

                foreach (var cityGroup in citiesGroup)
                {
                    var cityData = new CityAnalyticsDto
                    {
                        CityId = cityGroup.Key.CityId,
                        CityName = cityGroup.Key.CityName,
                        Streets = new List<StreetAnalyticsDto>()
                    };

                    var streetsGroup = cityGroup.GroupBy(o => new { o.StreetId, o.StreetName });

                    foreach (var streetGroup in streetsGroup)
                    {
                        var streetData = new StreetAnalyticsDto
                        {
                            StreetId = streetGroup.Key.StreetId,
                            StreetName = streetGroup.Key.StreetName,
                            Objects = new List<ObjectAnalyticsDto>()
                        };

                        foreach (var obj in streetGroup)
                        {
                            streetData.Objects.Add(new ObjectAnalyticsDto
                            {
                                ObjectId = obj.ObjectId,
                                Address = obj.StreetName,
                                HouseNumber = obj.HouseNumber,
                                ApartmentNumber = obj.ApartmentNumber,
                                ObjectType = obj.ObjectType,
                                Consumption = obj.Consumption,
                                Percentage = 0
                            });
                        }

                        streetData.TotalConsumption = streetData.Objects.Sum(o => o.Consumption);
                        streetData.ObjectsCount = streetData.Objects.Count;
                        streetData.AveragePerObject = streetData.ObjectsCount > 0
                            ? streetData.TotalConsumption / streetData.ObjectsCount
                            : 0;

                        cityData.Streets.Add(streetData);
                    }

                    cityData.TotalConsumption = cityData.Streets.Sum(s => s.TotalConsumption);
                    cityData.ObjectsCount = cityData.Streets.Sum(s => s.ObjectsCount);
                    cityData.AveragePerObject = cityData.ObjectsCount > 0
                        ? cityData.TotalConsumption / cityData.ObjectsCount
                        : 0;

                    regionData.Cities.Add(cityData);
                }

                regionData.TotalConsumption = regionData.Cities.Sum(c => c.TotalConsumption);
                regionData.ObjectsCount = regionData.Cities.Sum(c => c.ObjectsCount);
                regionData.AveragePerObject = regionData.ObjectsCount > 0
                    ? regionData.TotalConsumption / regionData.ObjectsCount
                    : 0;

                result.Add(regionData);
            }

            result = result.OrderByDescending(r => r.TotalConsumption).ToList();

            var totalConsumptionAll = result.Sum(r => r.TotalConsumption);
            foreach (var region in result)
            {
                region.Percentage = totalConsumptionAll > 0
                    ? (region.TotalConsumption / totalConsumptionAll) * 100
                    : 0;

                foreach (var city in region.Cities)
                {
                    city.Percentage = region.TotalConsumption > 0
                        ? (city.TotalConsumption / region.TotalConsumption) * 100
                        : 0;
                }
            }

            return result;
        }

        public async Task<RegionAnalyticsDto> GetAnalyticsByRegionIdAsync(int regionId, int year, int month)
        {
            var region = await Query<Region>().FirstOrDefaultAsync(r => r.Id == regionId);
            if (region == null) return null;

            var consumptionByObject = await GetConsumptionByObjectAsync(year, month);

            var objectsInRegion = consumptionByObject
                .Where(o => o.RegionId == regionId)
                .ToList();

            if (!objectsInRegion.Any()) return null;

            var result = new RegionAnalyticsDto
            {
                RegionId = region.Id,
                RegionName = region.Name,
                Cities = new List<CityAnalyticsDto>()
            };

            var citiesGroup = objectsInRegion.GroupBy(o => new { o.CityId, o.CityName });

            foreach (var cityGroup in citiesGroup)
            {
                var cityData = new CityAnalyticsDto
                {
                    CityId = cityGroup.Key.CityId,
                    CityName = cityGroup.Key.CityName,
                    Streets = new List<StreetAnalyticsDto>()
                };

                var streetsGroup = cityGroup.GroupBy(o => new { o.StreetId, o.StreetName });

                foreach (var streetGroup in streetsGroup)
                {
                    var streetData = new StreetAnalyticsDto
                    {
                        StreetId = streetGroup.Key.StreetId,
                        StreetName = streetGroup.Key.StreetName,
                        Objects = new List<ObjectAnalyticsDto>()
                    };

                    foreach (var obj in streetGroup)
                    {
                        streetData.Objects.Add(new ObjectAnalyticsDto
                        {
                            ObjectId = obj.ObjectId,
                            Address = obj.StreetName,
                            HouseNumber = obj.HouseNumber,
                            ApartmentNumber = obj.ApartmentNumber,
                            ObjectType = obj.ObjectType,
                            Consumption = obj.Consumption
                        });
                    }

                    streetData.TotalConsumption = streetData.Objects.Sum(o => o.Consumption);
                    streetData.ObjectsCount = streetData.Objects.Count;
                    streetData.AveragePerObject = streetData.ObjectsCount > 0
                        ? streetData.TotalConsumption / streetData.ObjectsCount
                        : 0;

                    cityData.Streets.Add(streetData);
                }

                cityData.TotalConsumption = cityData.Streets.Sum(s => s.TotalConsumption);
                cityData.ObjectsCount = cityData.Streets.Sum(s => s.ObjectsCount);
                cityData.AveragePerObject = cityData.ObjectsCount > 0
                    ? cityData.TotalConsumption / cityData.ObjectsCount
                    : 0;

                result.Cities.Add(cityData);
            }

            result.TotalConsumption = result.Cities.Sum(c => c.TotalConsumption);
            result.ObjectsCount = result.Cities.Sum(c => c.ObjectsCount);
            result.AveragePerObject = result.ObjectsCount > 0
                ? result.TotalConsumption / result.ObjectsCount
                : 0;

            foreach (var city in result.Cities)
            {
                city.Percentage = result.TotalConsumption > 0
                    ? (city.TotalConsumption / result.TotalConsumption) * 100
                    : 0;
            }

            return result;
        }

        public async Task<List<ObjectAnalyticsDto>> GetTopObjectsByRegionAsync(int regionId, int year, int month, int topCount = 10)
        {
            var consumptionByObject = await GetConsumptionByObjectAsync(year, month);

            var objectsInRegion = consumptionByObject
                .Where(o => o.RegionId == regionId)
                .OrderByDescending(o => o.Consumption)
                .Take(topCount)
                .ToList();

            var totalConsumption = objectsInRegion.Sum(o => o.Consumption);

            var result = new List<ObjectAnalyticsDto>();
            foreach (var obj in objectsInRegion)
            {
                result.Add(new ObjectAnalyticsDto
                {
                    ObjectId = obj.ObjectId,
                    Address = obj.StreetName,
                    HouseNumber = obj.HouseNumber,
                    ApartmentNumber = obj.ApartmentNumber,
                    ObjectType = obj.ObjectType,
                    Consumption = obj.Consumption,
                    Percentage = totalConsumption > 0
                        ? (obj.Consumption / totalConsumption) * 100
                        : 0
                });
            }

            return result;
        }

        private async Task<List<ObjectConsumptionDto>> GetConsumptionByObjectAsync(int year, int month)
        {
            var result = new List<ObjectConsumptionDto>();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var readingsForMonth = await Query<MeterReading>()
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToListAsync();

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

                var meter = await Query<Meter>()
                    .FirstOrDefaultAsync(m => m.Id == reading.MeterId);

                if (meter == null) continue;

                var obj = await Query<ConsumptionObject>()
                    .FirstOrDefaultAsync(o => o.Id == meter.ConsumptionObjectId);

                if (obj == null) continue;

                var street = await Query<Street>()
                    .FirstOrDefaultAsync(s => s.Id == obj.StreetId);

                var city = street != null ? await Query<City>()
                    .FirstOrDefaultAsync(c => c.Id == street.CityId) : null;

                var objectType = await Query<ObjectType>()
                    .FirstOrDefaultAsync(t => t.Id == obj.ObjectTypeId);

                result.Add(new ObjectConsumptionDto
                {
                    ObjectId = obj.Id,
                    RegionId = city?.RegionId ?? 0,
                    CityId = city?.Id ?? 0,
                    CityName = city?.Name ?? "Не указан",
                    StreetId = street?.Id ?? 0,
                    StreetName = street?.Name ?? "Не указана",
                    HouseNumber = obj.HouseNumber,
                    ApartmentNumber = obj.ApartmentNumber,
                    ObjectType = objectType?.Name ?? "Не указан",
                    Consumption = consumption
                });
            }

            return result;
        }

        private List<ObjectConsumptionDto> GetConsumptionByObject(int year, int month)
        {
            return GetConsumptionByObjectAsync(year, month).Result;
        }

        private class ObjectConsumptionDto
        {
            public int ObjectId { get; set; }
            public int RegionId { get; set; }
            public int CityId { get; set; }
            public string CityName { get; set; }
            public int StreetId { get; set; }
            public string StreetName { get; set; }
            public string HouseNumber { get; set; }
            public string ApartmentNumber { get; set; }
            public string ObjectType { get; set; }
            public decimal Consumption { get; set; }
        }
    }
}