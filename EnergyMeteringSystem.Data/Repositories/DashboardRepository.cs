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

        public async Task<List<ChartDataPointDto>> GetChartDataAsync(int year)
        {
            var result = new List<ChartDataPointDto>();
            string[] monthNames = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            try
            {
                // ✅ Упрощаем запрос — сначала получаем данные, потом группируем в памяти
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var readings = await _context.MeterReading
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .Select(r => new { r.ReadingDate })
                    .ToListAsync();

                // Группируем в памяти (здесь нет проблем с LINQ to Entities)
                var grouped = readings
                    .GroupBy(r => r.ReadingDate.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .ToDictionary(x => x.Month, x => x.Count);

                for (int month = 1; month <= 12; month++)
                {
                    result.Add(new ChartDataPointDto
                    {
                        MonthName = monthNames[month - 1],
                        Consumption = grouped.ContainsKey(month) ? grouped[month] : 0
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetChartDataAsync ERROR: {ex.Message}");
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