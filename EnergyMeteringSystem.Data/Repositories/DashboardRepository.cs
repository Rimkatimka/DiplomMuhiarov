using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime weekAgo = today.AddDays(-7);

                var totalObjects = await _context.ConsumptionObject.CountAsync();
                var totalMeters = await _context.Meter.CountAsync();
                var readingsToday = await _context.MeterReading.CountAsync(r => r.ReadingDate == today);
                var readingsWeek = await _context.MeterReading.CountAsync(r => r.ReadingDate >= weekAgo);
                var expiredMeters = await _context.Meter.CountAsync(m => m.NextVerificationDate < today);

                return new DashboardDto
                {
                    TotalObjects = totalObjects,
                    TotalMeters = totalMeters,
                    ReadingsToday = readingsToday,
                    ReadingsWeek = readingsWeek,
                    ExpiredMeters = expiredMeters
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDashboardDataAsync ERROR: {ex.Message}");
                return new DashboardDto();
            }
        }

        public DashboardDto GetDashboardData()
        {
            return GetDashboardDataAsync().Result;
        }

        /// <summary>
        /// Получить СУММУ ПОТРЕБЛЕНИЯ (разница между последним и предыдущим показанием) по месяцам за указанный год
        /// </summary>
        public async Task<List<ChartDataPointDto>> GetMonthlyConsumptionAsync(int year)
        {
            var result = new List<ChartDataPointDto>();
            string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                    "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            try
            {
                // Получаем все показания за год
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var readings = await _context.MeterReading
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .OrderBy(r => r.MeterId)
                    .ThenBy(r => r.ReadingDate)
                    .Select(r => new { r.MeterId, r.ReadingDate, r.Value })
                    .ToListAsync();

                // Для каждого счетчика вычисляем потребление по месяцам
                var monthlyConsumption = new Dictionary<int, decimal>();

                var groupedByMeter = readings.GroupBy(r => r.MeterId);

                foreach (var meterGroup in groupedByMeter)
                {
                    var orderedReadings = meterGroup.OrderBy(r => r.ReadingDate).ToList();

                    for (int i = 1; i < orderedReadings.Count; i++)
                    {
                        var prev = orderedReadings[i - 1];
                        var curr = orderedReadings[i];

                        decimal consumption = curr.Value - prev.Value;
                        if (consumption <= 0) continue;

                        int month = curr.ReadingDate.Month;

                        if (!monthlyConsumption.ContainsKey(month))
                            monthlyConsumption[month] = 0;
                        monthlyConsumption[month] += consumption;
                    }
                }

                // Формируем результат для всех 12 месяцев
                for (int month = 1; month <= 12; month++)
                {
                    decimal consumption = monthlyConsumption.ContainsKey(month) ? monthlyConsumption[month] : 0;
                    result.Add(new ChartDataPointDto
                    {
                        MonthName = monthNames[month - 1],
                        Consumption = consumption
                    });

                    System.Diagnostics.Debug.WriteLine($"  {monthNames[month - 1]}: {consumption:F0} кВт·ч");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMonthlyConsumptionAsync ERROR: {ex.Message}");
                // Возвращаем заглушку с нулями
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