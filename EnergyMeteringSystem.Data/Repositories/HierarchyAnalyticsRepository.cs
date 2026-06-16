using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class HierarchyAnalyticsRepository : BaseRepository
    {
        public List<RegionAnalyticsDto> GetAnalyticsByRegion(int year, int month)
        {
            System.Diagnostics.Debug.WriteLine($"GetAnalyticsByRegion: Year={year}, Month={month}");

            try
            {
                var data = (from r in _context.Region
                            join c in _context.City on r.Id equals c.RegionId
                            join s in _context.Street on c.Id equals s.CityId
                            join o in _context.ConsumptionObject on s.Id equals o.StreetId
                            join m in _context.Meter on o.Id equals m.ConsumptionObjectId
                            join mr in _context.MeterReading on m.Id equals mr.MeterId
                            where mr.ReadingDate.Year == year && mr.ReadingDate.Month == month
                            select new
                            {
                                RegionId = r.Id,
                                RegionName = r.Name,
                                CityId = c.Id,
                                CityName = c.Name,
                                StreetId = s.Id,
                                StreetName = s.Name,
                                ObjectId = o.Id,
                                HouseNumber = o.HouseNumber,
                                ApartmentNumber = o.ApartmentNumber,
                                ObjectTypeName = o.ObjectType.Name,
                                MeterId = m.Id,
                                ReadingValue = mr.Value,
                                InitialReading = m.InitialReading
                            })
                            .ToList();

                if (!data.Any())
                {
                    System.Diagnostics.Debug.WriteLine("  НЕТ ДАННЫХ ЗА ВЫБРАННЫЙ ПЕРИОД!");
                    return new List<RegionAnalyticsDto>();
                }

                // Группируем в памяти
                var grouped = data
                    .GroupBy(x => new { x.RegionId, x.RegionName, x.CityId, x.CityName, x.StreetId, x.StreetName, x.ObjectId, x.HouseNumber, x.ApartmentNumber, x.ObjectTypeName, x.MeterId, x.InitialReading })
                    .Select(g => new
                    {
                        g.Key.RegionId,
                        g.Key.RegionName,
                        g.Key.CityId,
                        g.Key.CityName,
                        g.Key.StreetId,
                        g.Key.StreetName,
                        g.Key.ObjectId,
                        g.Key.HouseNumber,
                        g.Key.ApartmentNumber,
                        g.Key.ObjectTypeName,
                        g.Key.InitialReading,
                        Consumption = g.Count() >= 2
                            ? g.Max(x => x.ReadingValue) - g.Min(x => x.ReadingValue)
                            : (g.FirstOrDefault() != null ? g.First().ReadingValue - g.First().InitialReading : 0)
                    })
                    .Where(x => x.Consumption > 0)
                    .ToList();

                if (!grouped.Any())
                {
                    System.Diagnostics.Debug.WriteLine("  НЕТ ОБЪЕКТОВ С ПОТРЕБЛЕНИЕМ > 0!");
                    return new List<RegionAnalyticsDto>();
                }

                // Строим иерархию
                var result = grouped
                    .GroupBy(x => new { x.RegionId, x.RegionName })
                    .Select(regionGroup => new RegionAnalyticsDto
                    {
                        RegionId = regionGroup.Key.RegionId,
                        RegionName = regionGroup.Key.RegionName,
                        Cities = regionGroup
                            .GroupBy(x => new { x.CityId, x.CityName })
                            .Select(cityGroup => new CityAnalyticsDto
                            {
                                CityId = cityGroup.Key.CityId,
                                CityName = cityGroup.Key.CityName,
                                Streets = cityGroup
                                    .GroupBy(x => new { x.StreetId, x.StreetName })
                                    .Select(streetGroup => new StreetAnalyticsDto
                                    {
                                        StreetId = streetGroup.Key.StreetId,
                                        StreetName = streetGroup.Key.StreetName,
                                        Objects = streetGroup.Select(x => new ObjectAnalyticsDto
                                        {
                                            ObjectId = x.ObjectId,
                                            Address = x.StreetName,
                                            HouseNumber = x.HouseNumber,
                                            ApartmentNumber = x.ApartmentNumber,
                                            ObjectType = x.ObjectTypeName,
                                            Consumption = x.Consumption
                                        }).ToList()
                                    }).ToList()
                            }).ToList()
                    })
                    .ToList();

                // Вычисляем суммы
                foreach (var region in result)
                {
                    region.TotalConsumption = region.Cities.Sum(c => c.Streets.Sum(s => s.Objects.Sum(o => o.Consumption)));
                    region.ObjectsCount = region.Cities.Sum(c => c.Streets.Sum(s => s.Objects.Count));
                    region.AveragePerObject = region.ObjectsCount > 0 ? region.TotalConsumption / region.ObjectsCount : 0;

                    foreach (var city in region.Cities)
                    {
                        city.TotalConsumption = city.Streets.Sum(s => s.Objects.Sum(o => o.Consumption));
                        city.ObjectsCount = city.Streets.Sum(s => s.Objects.Count);
                        city.AveragePerObject = city.ObjectsCount > 0 ? city.TotalConsumption / city.ObjectsCount : 0;
                    }
                }

                // Проценты
                var totalAll = result.Sum(r => r.TotalConsumption);
                foreach (var region in result)
                {
                    region.Percentage = totalAll > 0 ? (region.TotalConsumption / totalAll) * 100 : 0;
                    foreach (var city in region.Cities)
                    {
                        city.Percentage = region.TotalConsumption > 0 ? (city.TotalConsumption / region.TotalConsumption) * 100 : 0;
                        foreach (var street in city.Streets)
                        {
                            street.Percentage = city.TotalConsumption > 0 ? (street.TotalConsumption / city.TotalConsumption) * 100 : 0;
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"  ИТОГО: {result.Count} регионов");
                return result.OrderByDescending(r => r.TotalConsumption).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsByRegion ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return new List<RegionAnalyticsDto>();
            }
        }

        public RegionAnalyticsDto GetAnalyticsByRegionId(int regionId, int year, int month)
        {
            System.Diagnostics.Debug.WriteLine($"GetAnalyticsByRegionId: RegionId={regionId}, Year={year}, Month={month}");

            try
            {
                var data = (from c in _context.City
                            join s in _context.Street on c.Id equals s.CityId
                            join o in _context.ConsumptionObject on s.Id equals o.StreetId
                            join m in _context.Meter on o.Id equals m.ConsumptionObjectId
                            join mr in _context.MeterReading on m.Id equals mr.MeterId
                            where c.RegionId == regionId && mr.ReadingDate.Year == year && mr.ReadingDate.Month == month
                            select new
                            {
                                CityId = c.Id,
                                CityName = c.Name,
                                StreetId = s.Id,
                                StreetName = s.Name,
                                ObjectId = o.Id,
                                HouseNumber = o.HouseNumber,
                                ApartmentNumber = o.ApartmentNumber,
                                ObjectTypeName = o.ObjectType.Name,
                                MeterId = m.Id,
                                ReadingValue = mr.Value,
                                InitialReading = m.InitialReading
                            })
                            .ToList();

                if (!data.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"  НЕТ ДАННЫХ ДЛЯ РЕГИОНА {regionId} ЗА {month}.{year}");
                    return null;
                }

                var grouped = data
                    .GroupBy(x => new { x.CityId, x.CityName, x.StreetId, x.StreetName, x.ObjectId, x.HouseNumber, x.ApartmentNumber, x.ObjectTypeName, x.MeterId, x.InitialReading })
                    .Select(g => new
                    {
                        g.Key.CityId,
                        g.Key.CityName,
                        g.Key.StreetId,
                        g.Key.StreetName,
                        g.Key.ObjectId,
                        g.Key.HouseNumber,
                        g.Key.ApartmentNumber,
                        g.Key.ObjectTypeName,
                        g.Key.InitialReading,
                        Consumption = g.Count() >= 2
                            ? g.Max(x => x.ReadingValue) - g.Min(x => x.ReadingValue)
                            : (g.FirstOrDefault() != null ? g.First().ReadingValue - g.First().InitialReading : 0)
                    })
                    .Where(x => x.Consumption > 0)
                    .ToList();

                if (!grouped.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"  НЕТ ОБЪЕКТОВ С ПОТРЕБЛЕНИЕМ > 0 В РЕГИОНЕ {regionId}");
                    return null;
                }

                var regionName = _context.Region.Where(r => r.Id == regionId).Select(r => r.Name).FirstOrDefault() ?? "Неизвестно";

                var result = new RegionAnalyticsDto
                {
                    RegionId = regionId,
                    RegionName = regionName,
                    Cities = grouped
                        .GroupBy(x => new { x.CityId, x.CityName })
                        .Select(cityGroup => new CityAnalyticsDto
                        {
                            CityId = cityGroup.Key.CityId,
                            CityName = cityGroup.Key.CityName,
                            Streets = cityGroup
                                .GroupBy(x => new { x.StreetId, x.StreetName })
                                .Select(streetGroup => new StreetAnalyticsDto
                                {
                                    StreetId = streetGroup.Key.StreetId,
                                    StreetName = streetGroup.Key.StreetName,
                                    Objects = streetGroup.Select(x => new ObjectAnalyticsDto
                                    {
                                        ObjectId = x.ObjectId,
                                        Address = x.StreetName,
                                        HouseNumber = x.HouseNumber,
                                        ApartmentNumber = x.ApartmentNumber,
                                        ObjectType = x.ObjectTypeName,
                                        Consumption = x.Consumption
                                    }).ToList()
                                }).ToList()
                        }).ToList()
                };

                // Вычисляем суммы
                result.TotalConsumption = result.Cities.Sum(c => c.Streets.Sum(s => s.Objects.Sum(o => o.Consumption)));
                result.ObjectsCount = result.Cities.Sum(c => c.Streets.Sum(s => s.Objects.Count));
                result.AveragePerObject = result.ObjectsCount > 0 ? result.TotalConsumption / result.ObjectsCount : 0;

                foreach (var city in result.Cities)
                {
                    city.TotalConsumption = city.Streets.Sum(s => s.Objects.Sum(o => o.Consumption));
                    city.ObjectsCount = city.Streets.Sum(s => s.Objects.Count);
                    city.AveragePerObject = city.ObjectsCount > 0 ? city.TotalConsumption / city.ObjectsCount : 0;
                    city.Percentage = result.TotalConsumption > 0 ? (city.TotalConsumption / result.TotalConsumption) * 100 : 0;

                    foreach (var street in city.Streets)
                    {
                        street.TotalConsumption = street.Objects.Sum(o => o.Consumption);
                        street.ObjectsCount = street.Objects.Count;
                        street.AveragePerObject = street.ObjectsCount > 0 ? street.TotalConsumption / street.ObjectsCount : 0;
                        street.Percentage = city.TotalConsumption > 0 ? (street.TotalConsumption / city.TotalConsumption) * 100 : 0;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"  ИТОГО: {result.Cities.Count} городов в регионе {regionName}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAnalyticsByRegionId ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public List<ObjectAnalyticsDto> GetTopObjectsByRegion(int regionId, int year, int month, int topCount = 10)
        {
            System.Diagnostics.Debug.WriteLine($"GetTopObjectsByRegion: RegionId={regionId}, Year={year}, Month={month}");

            try
            {
                // ✅ ПОЛУЧАЕМ ДАННЫЕ ИЗ БД (без сложных вычислений)
                var rawData = (from o in _context.ConsumptionObject
                               join s in _context.Street on o.StreetId equals s.Id
                               join c in _context.City on s.CityId equals c.Id
                               join m in _context.Meter on o.Id equals m.ConsumptionObjectId
                               join mr in _context.MeterReading on m.Id equals mr.MeterId
                               where c.RegionId == regionId && mr.ReadingDate.Year == year && mr.ReadingDate.Month == month
                               select new
                               {
                                   ObjectId = o.Id,
                                   HouseNumber = o.HouseNumber,
                                   ApartmentNumber = o.ApartmentNumber,
                                   ObjectTypeName = o.ObjectType.Name,
                                   StreetName = s.Name,
                                   CityName = c.Name,
                                   MeterId = m.Id,
                                   ReadingValue = mr.Value,
                                   InitialReading = m.InitialReading,
                                   ReadingDate = mr.ReadingDate
                               })
                               .ToList();

                if (!rawData.Any())
                {
                    System.Diagnostics.Debug.WriteLine("  НЕТ ДАННЫХ ДЛЯ ТОП ОБЪЕКТОВ");
                    return new List<ObjectAnalyticsDto>();
                }

                // ✅ ГРУППИРУЕМ В ПАМЯТИ (здесь можно использовать First)
                var grouped = rawData
                    .GroupBy(x => new { x.ObjectId, x.HouseNumber, x.ApartmentNumber, x.ObjectTypeName, x.StreetName, x.CityName, x.MeterId, x.InitialReading })
                    .Select(g => new
                    {
                        g.Key.ObjectId,
                        g.Key.HouseNumber,
                        g.Key.ApartmentNumber,
                        g.Key.ObjectTypeName,
                        g.Key.StreetName,
                        g.Key.CityName,
                        g.Key.InitialReading,
                        Values = g.Select(x => x.ReadingValue).ToList(),
                        Dates = g.Select(x => x.ReadingDate).ToList()
                    })
                    .ToList();

                // ✅ ВЫЧИСЛЯЕМ ПОТРЕБЛЕНИЕ В ПАМЯТИ
                var result = new List<ObjectAnalyticsDto>();

                foreach (var item in grouped)
                {
                    decimal consumption = 0;
                    var sortedValues = item.Values.OrderBy(v => v).ToList();

                    if (sortedValues.Count >= 2)
                    {
                        consumption = sortedValues.Last() - sortedValues.First();
                    }
                    else if (sortedValues.Count == 1)
                    {
                        consumption = sortedValues.First() - item.InitialReading;
                    }

                    if (consumption > 0)
                    {
                        result.Add(new ObjectAnalyticsDto
                        {
                            ObjectId = item.ObjectId,
                            Address = item.StreetName + ", " + item.CityName,
                            HouseNumber = item.HouseNumber,
                            ApartmentNumber = item.ApartmentNumber,
                            ObjectType = item.ObjectTypeName,
                            Consumption = consumption
                        });
                    }
                }

                // ✅ СОРТИРУЕМ И БЕРЕМ ТОП
                var topResult = result
                    .OrderByDescending(x => x.Consumption)
                    .Take(topCount)
                    .ToList();

                var total = topResult.Sum(x => x.Consumption);
                foreach (var item in topResult)
                {
                    item.Percentage = total > 0 ? (item.Consumption / total) * 100 : 0;
                }

                System.Diagnostics.Debug.WriteLine($"GetTopObjectsByRegion: Найдено {topResult.Count} объектов");
                return topResult;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTopObjectsByRegion ОШИБКА: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return new List<ObjectAnalyticsDto>();
            }
        }
    }
}