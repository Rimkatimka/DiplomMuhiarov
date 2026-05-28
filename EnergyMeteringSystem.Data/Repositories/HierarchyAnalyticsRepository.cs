using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class HierarchyAnalyticsRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public HierarchyAnalyticsRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<RegionAnalyticsDto> GetAnalyticsByRegion(int year, int month)
        {
            var result = new List<RegionAnalyticsDto>();

            var regions = _context.Region.ToList();

            foreach (var region in regions)
            {
                var regionData = new RegionAnalyticsDto
                {
                    RegionId = region.Id,
                    RegionName = region.Name,
                    Cities = new List<CityAnalyticsDto>()
                };

                var citiesInRegion = _context.City.Where(c => c.RegionId == region.Id).ToList();

                foreach (var city in citiesInRegion)
                {
                    var cityData = new CityAnalyticsDto
                    {
                        CityId = city.Id,
                        CityName = city.Name,
                        Streets = new List<StreetAnalyticsDto>()
                    };

                    var streetsInCity = _context.Street.Where(s => s.CityId == city.Id).ToList();

                    foreach (var street in streetsInCity)
                    {
                        var streetData = new StreetAnalyticsDto
                        {
                            StreetId = street.Id,
                            StreetName = street.Name,
                            Objects = new List<ObjectAnalyticsDto>()
                        };

                        var objectsOnStreet = _context.ConsumptionObject
                            .Where(o => o.StreetId == street.Id)
                            .ToList();

                        foreach (var obj in objectsOnStreet)
                        {
                            var accrual = _context.Accrual
                                .FirstOrDefault(a => a.ConsumptionObjectId == obj.Id &&
                                                     a.PeriodYear == year &&
                                                     a.PeriodMonth == month);

                            if (accrual != null && accrual.ConsumptionValue > 0)
                            {
                                streetData.Objects.Add(new ObjectAnalyticsDto
                                {
                                    ObjectId = obj.Id,
                                    Address = street.Name,
                                    HouseNumber = obj.HouseNumber,
                                    ApartmentNumber = obj.ApartmentNumber,
                                    ObjectType = obj.ObjectType?.Name ?? "Неизвестно",
                                    Consumption = accrual.ConsumptionValue
                                });
                            }
                        }

                        if (streetData.Objects.Any())
                        {
                            streetData.TotalConsumption = streetData.Objects.Sum(o => o.Consumption);
                            streetData.ObjectsCount = streetData.Objects.Count;
                            streetData.AveragePerObject = streetData.TotalConsumption / streetData.ObjectsCount;

                            cityData.Streets.Add(streetData);
                        }
                    }

                    if (cityData.Streets.Any())
                    {
                        cityData.TotalConsumption = cityData.Streets.Sum(s => s.TotalConsumption);
                        cityData.ObjectsCount = cityData.Streets.Sum(s => s.ObjectsCount);
                        cityData.AveragePerObject = cityData.ObjectsCount > 0
                            ? cityData.TotalConsumption / cityData.ObjectsCount
                            : 0;

                        regionData.Cities.Add(cityData);
                    }
                }

                if (regionData.Cities.Any())
                {
                    regionData.TotalConsumption = regionData.Cities.Sum(c => c.TotalConsumption);
                    regionData.ObjectsCount = regionData.Cities.Sum(c => c.ObjectsCount);
                    regionData.AveragePerObject = regionData.ObjectsCount > 0
                        ? regionData.TotalConsumption / regionData.ObjectsCount
                        : 0;

                    result.Add(regionData);
                }
            }

            return result;
        }

        public RegionAnalyticsDto GetAnalyticsByRegionId(int regionId, int year, int month)
        {
            var region = _context.Region.FirstOrDefault(r => r.Id == regionId);
            if (region == null) return null;

            System.Diagnostics.Debug.WriteLine($"=== Аналитика для региона: {region.Name} (ID={regionId}) ===");

            var result = new RegionAnalyticsDto
            {
                RegionId = region.Id,
                RegionName = region.Name,
                Cities = new List<CityAnalyticsDto>()
            };

            var citiesInRegion = _context.City.Where(c => c.RegionId == regionId).ToList();
            System.Diagnostics.Debug.WriteLine($"Найдено городов: {citiesInRegion.Count}");

            foreach (var city in citiesInRegion)
            {
                System.Diagnostics.Debug.WriteLine($"  Город: {city.Name} (ID={city.Id})");

                var cityData = new CityAnalyticsDto
                {
                    CityId = city.Id,
                    CityName = city.Name,
                    Streets = new List<StreetAnalyticsDto>()
                };

                var streetsInCity = _context.Street.Where(s => s.CityId == city.Id).ToList();
                System.Diagnostics.Debug.WriteLine($"    Улиц в городе: {streetsInCity.Count}");

                foreach (var street in streetsInCity)
                {
                    System.Diagnostics.Debug.WriteLine($"      Улица: {street.Name} (ID={street.Id})");

                    var streetData = new StreetAnalyticsDto
                    {
                        StreetId = street.Id,
                        StreetName = street.Name,
                        Objects = new List<ObjectAnalyticsDto>()
                    };

                    var objectsOnStreet = _context.ConsumptionObject
                        .Where(o => o.StreetId == street.Id)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"        Объектов на улице: {objectsOnStreet.Count}");

                    foreach (var obj in objectsOnStreet)
                    {
                        var accrual = _context.Accrual
                            .FirstOrDefault(a => a.ConsumptionObjectId == obj.Id &&
                                                 a.PeriodYear == year &&
                                                 a.PeriodMonth == month);

                        if (accrual != null && accrual.ConsumptionValue > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"          Объект {obj.HouseNumber}: потребление {accrual.ConsumptionValue}");
                            streetData.Objects.Add(new ObjectAnalyticsDto
                            {
                                ObjectId = obj.Id,
                                Address = street.Name,
                                HouseNumber = obj.HouseNumber,
                                ApartmentNumber = obj.ApartmentNumber,
                                ObjectType = obj.ObjectType?.Name ?? "Неизвестно",
                                Consumption = accrual.ConsumptionValue
                            });
                        }
                    }

                    if (streetData.Objects.Any())
                    {
                        streetData.TotalConsumption = streetData.Objects.Sum(o => o.Consumption);
                        streetData.ObjectsCount = streetData.Objects.Count;
                        streetData.AveragePerObject = streetData.TotalConsumption / streetData.ObjectsCount;
                        cityData.Streets.Add(streetData);
                    }
                }

                if (cityData.Streets.Any())
                {
                    cityData.TotalConsumption = cityData.Streets.Sum(s => s.TotalConsumption);
                    cityData.ObjectsCount = cityData.Streets.Sum(s => s.ObjectsCount);
                    cityData.AveragePerObject = cityData.ObjectsCount > 0
                        ? cityData.TotalConsumption / cityData.ObjectsCount
                        : 0;
                    result.Cities.Add(cityData);
                }
            }

            result.TotalConsumption = result.Cities.Sum(c => c.TotalConsumption);
            result.ObjectsCount = result.Cities.Sum(c => c.ObjectsCount);
            result.AveragePerObject = result.ObjectsCount > 0
                ? result.TotalConsumption / result.ObjectsCount
                : 0;

            System.Diagnostics.Debug.WriteLine($"=== ИТОГО: {result.Cities.Count} городов, {result.ObjectsCount} объектов ===");

            return result;
        }
        public List<ObjectAnalyticsDto> GetTopObjectsByRegion(int regionId, int year, int month, int topCount = 10)
        {
            var objectsInRegion = from o in _context.ConsumptionObject
                                  join s in _context.Street on o.StreetId equals s.Id
                                  join c in _context.City on s.CityId equals c.Id
                                  where c.RegionId == regionId
                                  select o;

            var result = new List<ObjectAnalyticsDto>();

            foreach (var obj in objectsInRegion)
            {
                var accrual = _context.Accrual
                    .FirstOrDefault(a => a.ConsumptionObjectId == obj.Id &&
                                         a.PeriodYear == year &&
                                         a.PeriodMonth == month);

                if (accrual != null && accrual.ConsumptionValue > 0)
                {
                    var street = _context.Street.FirstOrDefault(s => s.Id == obj.StreetId);
                    result.Add(new ObjectAnalyticsDto
                    {
                        ObjectId = obj.Id,
                        Address = street?.Name ?? "Неизвестно",
                        HouseNumber = obj.HouseNumber,
                        ApartmentNumber = obj.ApartmentNumber,
                        ObjectType = obj.ObjectType?.Name ?? "Неизвестно",
                        Consumption = accrual.ConsumptionValue
                    });
                }
            }

            var totalConsumption = result.Sum(r => r.Consumption);
            foreach (var item in result)
            {
                item.Percentage = totalConsumption > 0
                    ? item.Consumption / totalConsumption * 100
                    : 0;
            }

            return result.OrderByDescending(r => r.Consumption).Take(topCount).ToList();
        }
    }
}