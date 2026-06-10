using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class DashboardRepository : BaseRepository, IDashboardRepository
    {
        // Синхронный (для совместимости)
        public DashboardDto GetDashboardData()
        {
            return GetDashboardDataAsync().Result;
        }

        // ✅ АСИНХРОННЫЙ
        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            DateTime today = DateTime.Today;
            DateTime weekAgo = today.AddDays(-7);

            var totalObjectsTask = Query<ConsumptionObject>().CountAsync();
            var totalMetersTask = Query<Meter>().CountAsync();
            var readingsTodayTask = Query<MeterReading>().CountAsync(r => r.ReadingDate == today);
            var readingsWeekTask = Query<MeterReading>().CountAsync(r => r.ReadingDate >= weekAgo);
            var expiredMetersTask = Query<Meter>().CountAsync(m => m.NextVerificationDate < today);

            await Task.WhenAll(totalObjectsTask, totalMetersTask, readingsTodayTask, readingsWeekTask, expiredMetersTask);

            DashboardDto result = new()
            {
                TotalObjects = await totalObjectsTask,
                TotalMeters = await totalMetersTask,
                ReadingsToday = await readingsTodayTask,
                ReadingsWeek = await readingsWeekTask,
                ExpiredMeters = await expiredMetersTask,
                ConsumptionChart = await GetChartDataLegacyAsync()
            };

            return result;
        }

        public List<ChartDataPointDto> GetChartData(int year)
        {
            return GetChartDataAsync(year).Result;
        }

        public async Task<List<ChartDataPointDto>> GetChartDataAsync(int year)
        {
            var result = new List<ChartDataPointDto>();
            string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            for (int month = 1; month <= 12; month++)
            {
                decimal consumption = await GetConsumptionByMonthAsync(year, month);
                result.Add(new ChartDataPointDto
                {
                    MonthName = months[month - 1],
                    Consumption = consumption
                });
            }

            return result;
        }

        private async Task<List<ChartPoint>> GetChartDataLegacyAsync()
        {
            List<ChartPoint> result = [];
            DateTime today = DateTime.Today;

            for (int i = 5; i >= 0; i--)
            {
                DateTime date = today.AddMonths(-i);
                int year = date.Year;
                int month = date.Month;

                decimal consumption = await GetConsumptionByMonthAsync(year, month);

                result.Add(new ChartPoint
                {
                    Label = GetMonthName(month),
                    Value = consumption
                });
            }

            return result;
        }

        private List<ChartPoint> GetChartDataLegacy()
        {
            return GetChartDataLegacyAsync().Result;
        }

        private async Task<decimal> GetConsumptionByMonthAsync(int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var readingsForMonth = await Query<MeterReading>()
                .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToListAsync();

            decimal totalConsumption = 0;

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

                if (consumption > 0)
                    totalConsumption += consumption;
            }

            return totalConsumption;
        }

        private decimal GetConsumptionByMonth(int year, int month)
        {
            return GetConsumptionByMonthAsync(year, month).Result;
        }

        private string GetMonthName(int month)
        {
            string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
            return months[month - 1];
        }
    }
}