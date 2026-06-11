using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class HierarchyAnalyticsRepository : BaseRepository
    {
        // Константы для кэширования
        private const string CACHE_KEY_PREFIX = "HierarchyAnalytics_";
        private const int CACHE_MINUTES = 30;

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

        // ✅ ОПТИМИЗИРОВАННЫЙ метод
        public async Task<List<RegionAnalyticsDto>> GetAnalyticsByRegionAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"{CACHE_KEY_PREFIX}Region_{year}_{month}";

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var consumptionData = await GetConsumptionByObjectOptimizedAsync(year, month, cancellationToken);

                var regions = await Query<Region>()
                    .OrderBy(r => r.Name)
                    .ToListAsync(cancellationToken);

                var result = new List<RegionAnalyticsDto>();

                foreach (var region in regions)
                {
                    var objectsInRegion = consumptionData
                        .Where(o => o.RegionId == region.Id)
                        .ToList();

                    if (!objectsInRegion.Any()) continue;

                    var regionData = new RegionAnalyticsDto
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
                                Objects = streetGroup.Select(obj => new ObjectAnalyticsDto
                                {
                                    ObjectId = obj.ObjectId,
                                    Address = obj.StreetName,
                                    HouseNumber = obj.HouseNumber,
                                    ApartmentNumber = obj.ApartmentNumber,
                                    ObjectType = obj.ObjectType,
                                    Consumption = obj.Consumption,
                                    Percentage = 0
                                }).ToList()
                            };

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
            }, CACHE_MINUTES);
        }

        public async Task<RegionAnalyticsDto> GetAnalyticsByRegionIdAsync(int regionId, int year, int month, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"{CACHE_KEY_PREFIX}RegionId_{regionId}_{year}_{month}";

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var region = await Query<Region>()
                    .FirstOrDefaultAsync(r => r.Id == regionId, cancellationToken);

                if (region == null) return null;

                var consumptionData = await GetConsumptionByObjectOptimizedAsync(year, month, cancellationToken);

                var objectsInRegion = consumptionData
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
                            Objects = streetGroup.Select(obj => new ObjectAnalyticsDto
                            {
                                ObjectId = obj.ObjectId,
                                Address = obj.StreetName,
                                HouseNumber = obj.HouseNumber,
                                ApartmentNumber = obj.ApartmentNumber,
                                ObjectType = obj.ObjectType,
                                Consumption = obj.Consumption
                            }).ToList()
                        };

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
            }, CACHE_MINUTES);
        }

        public async Task<List<ObjectAnalyticsDto>> GetTopObjectsByRegionAsync(int regionId, int year, int month, int topCount = 10, CancellationToken cancellationToken = default)
        {
            var consumptionData = await GetConsumptionByObjectOptimizedAsync(year, month, cancellationToken);

            var objectsInRegion = consumptionData
                .Where(o => o.RegionId == regionId)
                .OrderByDescending(o => o.Consumption)
                .Take(topCount)
                .ToList();

            var totalConsumption = objectsInRegion.Sum(o => o.Consumption);

            return objectsInRegion.Select(obj => new ObjectAnalyticsDto
            {
                ObjectId = obj.ObjectId,
                Address = obj.StreetName,
                HouseNumber = obj.HouseNumber,
                ApartmentNumber = obj.ApartmentNumber,
                ObjectType = obj.ObjectType,
                Consumption = obj.Consumption,
                Percentage = totalConsumption > 0 ? (obj.Consumption / totalConsumption) * 100 : 0
            }).ToList();
        }

        // ✅ ИСПРАВЛЕННЫЙ метод - используем Include вместо Join
        private async Task<List<ObjectConsumptionDto>> GetConsumptionByObjectOptimizedAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Получаем показания за месяц с навигационными свойствами
            var readings = await Query<MeterReading>()
                .Include(r => r.Meter)
                .Include(r => r.Meter.ConsumptionObject)
                .Include(r => r.Meter.ConsumptionObject.Street)
                .Include(r => r.Meter.ConsumptionObject.Street.City)
                .Include(r => r.Meter.ConsumptionObject.Street.City.Region)
                .Include(r => r.Meter.ConsumptionObject.ObjectType)
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .ToListAsync(cancellationToken);

            // Группируем по счетчикам и берем последнее показание
            var lastReadings = readings
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).First())
                .ToList();

            if (!lastReadings.Any()) return new List<ObjectConsumptionDto>();

            // Получаем ID всех счетчиков
            var meterIds = lastReadings.Select(r => r.MeterId).ToList();

            // Получаем предыдущие показания
            var previousReadings = await GetPreviousReadingsBatchAsync(meterIds, startDate, cancellationToken);

            var result = new List<ObjectConsumptionDto>();

            foreach (var reading in lastReadings)
            {
                if (reading?.Meter?.ConsumptionObject == null) continue;

                var obj = reading.Meter.ConsumptionObject;
                var street = obj.Street;
                var city = street?.City;
                var region = city?.Region;
                var objectType = obj.ObjectType;

                previousReadings.TryGetValue(reading.MeterId, out var prevReading);

                decimal consumption = prevReading != null
                    ? reading.Value - prevReading.Value
                    : reading.Value;

                if (consumption <= 0) continue;

                result.Add(new ObjectConsumptionDto
                {
                    ObjectId = obj.Id,
                    RegionId = region?.Id ?? 0,
                    RegionName = region?.Name ?? "Не указан",
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

        // Оставляем старый метод для совместимости
        [Obsolete("Используйте GetConsumptionByObjectOptimizedAsync")]
        private async Task<List<ObjectConsumptionDto>> GetConsumptionByObjectAsync(int year, int month)
        {
            return await GetConsumptionByObjectOptimizedAsync(year, month);
        }

        private class ObjectConsumptionDto
        {
            public int ObjectId { get; set; }
            public int RegionId { get; set; }
            public string RegionName { get; set; }
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