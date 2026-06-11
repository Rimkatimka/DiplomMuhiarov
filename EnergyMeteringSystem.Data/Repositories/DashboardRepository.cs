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
        public DashboardDto GetDashboardData()
        {
            return GetDashboardDataAsync().Result;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime weekAgo = today.AddDays(-7);

                var totalObjects = await Query<ConsumptionObject>().CountAsync(cancellationToken);
                var totalMeters = await Query<Meter>().CountAsync(cancellationToken);
                var readingsToday = await Query<MeterReading>().CountAsync(r => r.ReadingDate == today, cancellationToken);
                var readingsWeek = await Query<MeterReading>().CountAsync(r => r.ReadingDate >= weekAgo, cancellationToken);
                var expiredMeters = await Query<Meter>().CountAsync(m => m.NextVerificationDate < today, cancellationToken);

                return new DashboardDto
                {
                    TotalObjects = totalObjects,
                    TotalMeters = totalMeters,
                    ReadingsToday = readingsToday,
                    ReadingsWeek = readingsWeek,
                    ExpiredMeters = expiredMeters,
                    ConsumptionChart = new List<ChartPoint>()
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDashboardDataAsync ERROR: {ex.Message}");
                return new DashboardDto();
            }
        }

        // ✅ ГЛАВНЫЙ МЕТОД ДЛЯ ГРАФИКА - работает всегда
        public async Task<List<ChartDataPointDto>> GetChartDataAsync(int year)
        {
            var result = new List<ChartDataPointDto>();
            string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            try
            {
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                // Получаем данные по месяцам
                var readingsByMonth = await Query<MeterReading>()
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .GroupBy(r => r.ReadingDate.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToListAsync();

                var monthlyDict = readingsByMonth.ToDictionary(x => x.Month, x => x.Count);

                for (int month = 1; month <= 12; month++)
                {
                    result.Add(new ChartDataPointDto
                    {
                        MonthName = monthNames[month - 1],
                        Consumption = monthlyDict.ContainsKey(month) ? monthlyDict[month] : 0
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetChartDataAsync ERROR: {ex.Message}");
                // Возвращаем пустые данные, но с названиями месяцев
                for (int month = 1; month <= 12; month++)
                {
                    result.Add(new ChartDataPointDto
                    {
                        MonthName = monthNames[month - 1],
                        Consumption = 0
                    });
                }
            }

            return result;
        }
    }
}