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
    public class ReportRepository : BaseRepository
    {
        private static readonly string[] MonthNames = {
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        };

        // Константы для кэширования
        private const string CACHE_KEY_CONSUMPTION_REPORT = "ConsumptionReport_{0}_{1}_{2}_{3}";
        private const int CACHE_MINUTES = 15;

        public List<MonthlyConsumptionDto> GetMonthlyConsumption(int year)
        {
            return GetMonthlyConsumptionAsync(year).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ метод - ОДИН ЗАПРОС для всех месяцев
        public async Task<List<MonthlyConsumptionDto>> GetMonthlyConsumptionOptimizedAsync(int year, CancellationToken cancellationToken = default)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            // Получаем все показания за год
            var allReadings = await Query<MeterReading>()
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .OrderBy(r => r.MeterId)
                .ThenBy(r => r.ReadingDate)
                .ToListAsync(cancellationToken);

            // Группируем по счетчикам
            var readingsByMeter = allReadings
                .GroupBy(r => r.MeterId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var monthlyTotals = new Dictionary<int, decimal>();
            for (int m = 1; m <= 12; m++)
                monthlyTotals[m] = 0;

            foreach (var meterReadings in readingsByMeter.Values)
            {
                if (meterReadings.Count < 2) continue;

                for (int i = 1; i < meterReadings.Count; i++)
                {
                    var prev = meterReadings[i - 1];
                    var curr = meterReadings[i];
                    var consumption = curr.Value - prev.Value;

                    if (consumption > 0)
                    {
                        int month = curr.ReadingDate.Month;
                        monthlyTotals[month] += consumption;
                    }
                }
            }

            return monthlyTotals.Select(m => new MonthlyConsumptionDto
            {
                Year = year,
                Month = m.Key,
                MonthName = MonthNames[m.Key - 1],
                Consumption = m.Value
            }).ToList();
        }

        [Obsolete("Используйте GetMonthlyConsumptionOptimizedAsync")]
        public async Task<List<MonthlyConsumptionDto>> GetMonthlyConsumptionAsync(int year)
        {
            return await GetMonthlyConsumptionOptimizedAsync(year);
        }

        public List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate)
        {
            return GetConsumptionReportAsync(startDate, endDate).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ метод - ОДИН ЗАПРОС!
        public async Task<List<ConsumptionReportDto>> GetConsumptionReportOptimizedAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            try
            {
                string cacheKey = string.Format(CACHE_KEY_CONSUMPTION_REPORT,
                    startDate.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"));

                return await CacheService.GetOrAddAsync(cacheKey, async () =>
                {
                    // 🚀 ОДИН ГИГАНТСКИЙ JOIN запрос - ИСПРАВЛЕННЫЙ
                    var query = await (from r in Query<MeterReading>()
                                       join m in Query<Meter>() on r.MeterId equals m.Id
                                       join o in Query<ConsumptionObject>() on m.ConsumptionObjectId equals o.Id
                                       join s in Query<Street>() on o.StreetId equals s.Id
                                       join c in Query<City>() on s.CityId equals c.Id
                                       join ot in Query<ObjectType>() on o.ObjectTypeId equals ot.Id
                                       where r.ReadingDate >= startDate && r.ReadingDate <= endDate
                                       orderby r.ReadingDate
                                       select new ConsumptionReportTemp
                                       {
                                           MeterId = r.MeterId,
                                           Value = r.Value,
                                           ReadingDate = r.ReadingDate,
                                           ObjectId = o.Id,
                                           HouseNumber = o.HouseNumber,
                                           ApartmentNumber = o.ApartmentNumber,
                                           StreetName = s.Name,
                                           CityName = c.Name,
                                           ObjectTypeName = ot.Name,
                                           SerialNumber = m.SerialNumber,
                                           InitialReading = m.InitialReading,
                                           InstallationDate = m.InstallationDate
                                       })
                                       .ToListAsync(cancellationToken);

                    // Группируем по объектам
                    var result = new List<ConsumptionReportDto>();

                    foreach (var objGroup in query.GroupBy(x => x.ObjectId))
                    {
                        var firstReading = objGroup.OrderBy(x => x.ReadingDate).First();
                        var lastReading = objGroup.OrderByDescending(x => x.ReadingDate).First();

                        var consumption = lastReading.Value - firstReading.Value;
                        if (consumption <= 0) continue;

                        var meterReadings = objGroup.GroupBy(x => x.MeterId);
                        var meterSerial = meterReadings.First().First().SerialNumber;

                        string fullAddress = $"{firstReading.CityName}, {firstReading.StreetName}, {firstReading.HouseNumber}";
                        if (!string.IsNullOrEmpty(firstReading.ApartmentNumber))
                            fullAddress += $"/{firstReading.ApartmentNumber}";

                        result.Add(new ConsumptionReportDto
                        {
                            ObjectId = objGroup.Key,
                            Address = fullAddress,
                            MeterSerial = meterSerial ?? "Нет счетчика",
                            StartDate = firstReading.ReadingDate,
                            EndDate = lastReading.ReadingDate,
                            StartValue = firstReading.Value,
                            EndValue = lastReading.Value,
                            Consumption = consumption,
                            ObjectType = firstReading.ObjectTypeName ?? "Не указан"
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"GetConsumptionReportOptimizedAsync: loaded {result.Count} records");
                    return result.OrderByDescending(r => r.Consumption).ToList();
                }, CACHE_MINUTES);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetConsumptionReportOptimizedAsync: {ex.Message}");
                return new List<ConsumptionReportDto>();
            }
        }

        // Временный класс для данных
        private class ConsumptionReportTemp
        {
            public int MeterId { get; set; }
            public decimal Value { get; set; }
            public DateTime ReadingDate { get; set; }
            public int ObjectId { get; set; }
            public string HouseNumber { get; set; }
            public string ApartmentNumber { get; set; }
            public string StreetName { get; set; }
            public string CityName { get; set; }
            public string ObjectTypeName { get; set; }
            public string SerialNumber { get; set; }
            public decimal InitialReading { get; set; }
            public DateTime InstallationDate { get; set; }
        }

        [Obsolete("Используйте GetConsumptionReportOptimizedAsync")]
        public async Task<List<ConsumptionReportDto>> GetConsumptionReportAsync(DateTime startDate, DateTime endDate)
        {
            return await GetConsumptionReportOptimizedAsync(startDate, endDate);
        }

        public class MonthlyConsumptionDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string MonthName { get; set; }
            public decimal Consumption { get; set; }
        }
    }
}