using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public DashboardRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public DashboardDto GetDashboardData()
        {
            DateTime today = DateTime.Today;
            DateTime weekAgo = today.AddDays(-7);
            DateTime monthStart = new(today.Year, today.Month, 1);

            DashboardDto result = new()
            {
                TotalObjects = _context.ConsumptionObject.Count(),
                TotalMeters = _context.Meter.Count(),
                ReadingsToday = _context.MeterReading.Count(r => r.ReadingDate == today),
                ReadingsWeek = _context.MeterReading.Count(r => r.ReadingDate >= weekAgo),
                AccrualMonth = _context.Accrual.Where(a => a.PeriodYear == today.Year && a.PeriodMonth == today.Month).Sum(a => (decimal?)a.Amount) ?? 0,
                PaymentMonth = _context.Payment.Where(p => p.PaymentDate >= monthStart).Sum(p => (decimal?)p.Amount) ?? 0,
                ExpiredMeters = _context.Meter.Count(m => m.NextVerificationDate < today)
            };

            PaymentRepository paymentRepo = new();
            List<DebtDto> allDebtors = paymentRepo.GetDebtors();
            result.TopDebtors = allDebtors.Take(5).ToList();
            result.ConsumptionChart = GetChartDataLegacy();

            return result;
        }

        public List<ChartDataPointDto> GetChartData(int year)
        {
            var result = new List<ChartDataPointDto>();
            string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            for (int month = 1; month <= 12; month++)
            {
                decimal consumption = _context.Accrual
                    .Where(a => a.PeriodYear == year && a.PeriodMonth == month)
                    .Sum(a => (decimal?)a.ConsumptionValue) ?? 0;

                result.Add(new ChartDataPointDto
                {
                    MonthName = months[month - 1],
                    Consumption = consumption
                });
            }

            return result;
        }

        private List<ChartPoint> GetChartDataLegacy()
        {
            List<ChartPoint> result = [];
            DateTime today = DateTime.Today;

            for (int i = 5; i >= 0; i--)
            {
                DateTime date = today.AddMonths(-i);
                int year = date.Year;
                int month = date.Month;

                decimal consumption = _context.Accrual
                    .Where(a => a.PeriodYear == year && a.PeriodMonth == month)
                    .Sum(a => (decimal?)a.ConsumptionValue) ?? 0;

                result.Add(new ChartPoint
                {
                    Label = GetMonthName(month),
                    Value = consumption
                });
            }

            return result;
        }

        private string GetMonthName(int month)
        {
            string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                                "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
            return months[month - 1];
        }
    }
}