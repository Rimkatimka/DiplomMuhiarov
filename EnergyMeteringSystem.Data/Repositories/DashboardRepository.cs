using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var today = DateTime.Today;
            var weekAgo = today.AddDays(-7);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var result = new DashboardDto();

            // Количество объектов
            result.TotalObjects = _context.ConsumptionObject.Count();

            // Количество счетчиков
            result.TotalMeters = _context.Meter.Count();

            // Показания за сегодня
            result.ReadingsToday = _context.MeterReading
                .Count(r => r.ReadingDate == today);

            // Показания за неделю
            result.ReadingsWeek = _context.MeterReading
                .Count(r => r.ReadingDate >= weekAgo);

            // Начисления за текущий месяц
            result.AccrualMonth = _context.Accrual
                .Where(a => a.PeriodYear == today.Year &&
                            a.PeriodMonth == today.Month)
                .Sum(a => (decimal?)a.Amount) ?? 0;

            // Оплаты за текущий месяц
            result.PaymentMonth = _context.Payment
                .Where(p => p.PaymentDate >= monthStart)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            // Счетчики с истекшей поверкой
            result.ExpiredMeters = _context.Meter
                .Count(m => m.NextVerificationDate < today);

            // ТОП-5 должников
            var paymentRepo = new PaymentRepository();
            var allDebtors = paymentRepo.GetDebtors();
            result.TopDebtors = allDebtors.Take(5).ToList();

            // Данные для графика (последние 6 месяцев)
            result.ConsumptionChart = GetChartData();

            return result;
        }

        private List<ChartPoint> GetChartData()
        {
            var result = new List<ChartPoint>();
            var today = DateTime.Today;

            for (int i = 5; i >= 0; i--)
            {
                var date = today.AddMonths(-i);
                int year = date.Year;
                int month = date.Month;

                var consumption = _context.Accrual
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
