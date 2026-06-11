using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        // Константы для кэширования
        private const string CACHE_KEY_DASHBOARD = "DashboardData";
        private const string CACHE_KEY_CHART_DATA = "ChartData_{0}";
        private const int CACHE_MINUTES_DASHBOARD = 5;
        private const int CACHE_MINUTES_CHART = 30;

        // Массив полных названий месяцев (единый источник)
        private static readonly string[] MonthNames = {
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        };

        // Массив коротких названий для графика
        private static readonly string[] MonthNamesShort = {
            "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
            "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек"
        };

        // Синхронный (для совместимости)
        public DashboardDto GetDashboardData()
        {
            return GetDashboardDataAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ ДАШБОРД с кэшированием
        public async Task<DashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_DASHBOARD, async () =>
            {
                DateTime today = DateTime.Today;
                DateTime weekAgo = today.AddDays(-7);

                // Параллельное выполнение всех COUNT запросов
                var totalObjectsTask = Query<ConsumptionObject>().CountAsync(cancellationToken);
                var totalMetersTask = Query<Meter>().CountAsync(cancellationToken);
                var readingsTodayTask = Query<MeterReading>().CountAsync(r => r.ReadingDate == today, cancellationToken);
                var readingsWeekTask = Query<MeterReading>().CountAsync(r => r.ReadingDate >= weekAgo, cancellationToken);
                var expiredMetersTask = Query<Meter>().CountAsync(m => m.NextVerificationDate < today, cancellationToken);

                await Task.WhenAll(totalObjectsTask, totalMetersTask, readingsTodayTask, readingsWeekTask, expiredMetersTask);

                DashboardDto result = new DashboardDto
                {
                    TotalObjects = await totalObjectsTask,
                    TotalMeters = await totalMetersTask,
                    ReadingsToday = await readingsTodayTask,
                    ReadingsWeek = await readingsWeekTask,
                    ExpiredMeters = await expiredMetersTask,
                    ConsumptionChart = await GetChartDataLegacyAsync(cancellationToken)
                };

                return result;
            }, CACHE_MINUTES_DASHBOARD);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ график (возвращает ChartDataPointDto для ViewModel)
        public async Task<List<ChartDataPointDto>> GetChartDataOptimizedAsync(CancellationToken cancellationToken = default)
        {
            int year = DateTime.Today.Year;
            string cacheKey = string.Format(CACHE_KEY_CHART_DATA, year);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var result = new List<ChartDataPointDto>();

                // Один запрос для получения потребления за все месяцы года
                var monthlyConsumption = await GetMonthlyConsumptionBatchAsync(year, cancellationToken);

                for (int month = 1; month <= 12; month++)
                {
                    result.Add(new ChartDataPointDto
                    {
                        MonthName = MonthNames[month - 1],  // Полные названия
                        Consumption = monthlyConsumption.ContainsKey(month) ? monthlyConsumption[month] : 0
                    });
                }

                return result;
            }, CACHE_MINUTES_CHART);
        }

        // Синхронный метод для совместимости
        public List<ChartDataPointDto> GetChartData(int year)
        {
            return GetChartDataOptimizedAsync().Result;
        }

        public async Task<List<ChartDataPointDto>> GetChartDataAsync(int year)
        {
            return await GetChartDataOptimizedAsync();
        }

        // ✅ Метод для ChartPoint (использует короткие названия)
        private async Task<List<ChartPoint>> GetChartDataLegacyAsync(CancellationToken cancellationToken = default)
        {
            var chartData = await GetChartDataOptimizedAsync(cancellationToken);

            return chartData.Select((c, index) => new ChartPoint
            {
                Label = MonthNamesShort[index],  // Короткие названия для графика
                Value = c.Consumption
            }).ToList();
        }

        // ✅ НОВЫЙ МЕТОД: получение потребления за все месяцы года одним запросом
        private async Task<Dictionary<int, decimal>> GetMonthlyConsumptionBatchAsync(int year, CancellationToken cancellationToken = default)
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

            // Инициализируем словарь для всех месяцев
            for (int m = 1; m <= 12; m++)
                monthlyTotals[m] = 0;

            foreach (var meterReadings in readingsByMeter.Values)
            {
                if (meterReadings.Count < 2) continue;

                // Проходим по всем показаниям счетчика
                for (int i = 1; i < meterReadings.Count; i++)
                {
                    var prev = meterReadings[i - 1];
                    var curr = meterReadings[i];

                    var consumption = curr.Value - prev.Value;
                    if (consumption <= 0) continue;

                    // Определяем месяц потребления (по дате текущего показания)
                    int month = curr.ReadingDate.Month;
                    monthlyTotals[month] += consumption;
                }
            }

            return monthlyTotals;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ метод получения потребления за месяц
        private async Task<decimal> GetConsumptionByMonthOptimizedAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            // Получаем последние показания за месяц по каждому счетчику
            var consumptionData = await Query<MeterReading>()
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .GroupBy(r => r.MeterId)
                .Select(g => new
                {
                    MeterId = g.Key,
                    LastReading = g.OrderByDescending(r => r.ReadingDate).FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            // Получаем ID всех счетчиков
            var meterIds = consumptionData.Select(x => x.MeterId).ToList();

            // Один запрос для получения предыдущих показаний всех счетчиков
            var previousReadings = await GetPreviousReadingsBatchAsync(meterIds, startDate, cancellationToken);

            decimal totalConsumption = 0;

            foreach (var item in consumptionData)
            {
                if (item.LastReading == null) continue;

                previousReadings.TryGetValue(item.MeterId, out var prevReading);

                decimal consumption = prevReading != null
                    ? item.LastReading.Value - prevReading.Value
                    : item.LastReading.Value;

                if (consumption > 0)
                    totalConsumption += consumption;
            }

            return totalConsumption;
        }
    }
}