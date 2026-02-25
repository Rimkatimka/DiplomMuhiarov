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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public PaymentRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<PaymentDto> GetByPeriod(int year, int month)
        {
            return _context.Payment
                .Where(p => p.PeriodYear == year && p.PeriodMonth == month)
                .Join(_context.ConsumptionObject,
                    p => p.ConsumptionObjectId,
                    o => o.Id,
                    (p, o) => new PaymentDto
                    {
                        Id = p.Id,
                        ConsumptionObjectId = p.ConsumptionObjectId,
                        Address = o.Street.Name + ", д. " + o.HouseNumber +
                                  (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                        PaymentDate = p.PaymentDate,
                        Amount = p.Amount,
                        PaymentMethodName = p.PaymentMethod.Name,
                        ReceivedBy = p.User.FullName,
                        ReceiptNumber = p.ReceiptNumber,
                        PeriodMonth = p.PeriodMonth,
                        PeriodYear = p.PeriodYear
                    })
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public List<PaymentDto> GetByObjectId(int objectId)
        {
            return _context.Payment
                .Where(p => p.ConsumptionObjectId == objectId)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    ConsumptionObjectId = p.ConsumptionObjectId,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    PaymentMethodName = p.PaymentMethod.Name,
                    ReceivedBy = p.User.FullName,
                    ReceiptNumber = p.ReceiptNumber,
                    PeriodMonth = p.PeriodMonth,
                    PeriodYear = p.PeriodYear
                })
                .OrderByDescending(p => p.PaymentDate)
                .ToList();
        }

        public PaymentDto GetById(int id)
        {
            var p = _context.Payment
                .Include("PaymentMethod")
                .Include("User")
                .FirstOrDefault(x => x.Id == id);

            if (p == null) return null;

            return new PaymentDto
            {
                Id = p.Id,
                ConsumptionObjectId = p.ConsumptionObjectId,
                PaymentDate = p.PaymentDate,
                Amount = p.Amount,
                PaymentMethodName = p.PaymentMethod.Name,
                ReceivedBy = p.User.FullName,
                ReceiptNumber = p.ReceiptNumber,
                PeriodMonth = p.PeriodMonth,
                PeriodYear = p.PeriodYear
            };
        }

        public void Add(PaymentRegistrationDto dto)
        {
            var entity = new Payment
            {
                ConsumptionObjectId = dto.ConsumptionObjectId,
                PaymentDate = DateTime.Now,
                Amount = dto.Amount,
                PaymentMethodId = dto.PaymentMethodId,
                ReceivedByUserId = dto.ReceivedByUserId,
                ReceiptNumber = dto.ReceiptNumber,
                PeriodMonth = dto.PeriodMonth,
                PeriodYear = dto.PeriodYear
            };

            _context.Payment.Add(entity);
            _context.SaveChanges();
        }

        public decimal GetTotalForPeriod(int year, int month)
        {
            return _context.Payment
                .Where(p => p.PeriodYear == year && p.PeriodMonth == month)
                .Sum(p => (decimal?)p.Amount) ?? 0;
        }

        public List<DebtDto> GetDebtors()
        {
            var result = new List<DebtDto>();
            var currentDate = DateTime.Now;

            // Берем объекты с начислениями за последние 3 месяца
            var objects = _context.ConsumptionObject.ToList();

            foreach (var obj in objects)
            {
                decimal totalDebt = 0;
                int lastMonthWithDebt = 0;
                int lastYearWithDebt = 0;

                // Проверяем последние 6 месяцев
                for (int i = 1; i <= 6; i++)
                {
                    var periodDate = currentDate.AddMonths(-i);
                    int year = periodDate.Year;
                    int month = periodDate.Month;

                    var accrual = _context.Accrual
                        .FirstOrDefault(a => a.ConsumptionObjectId == obj.Id &&
                                             a.PeriodYear == year &&
                                             a.PeriodMonth == month);

                    if (accrual != null && !accrual.IsPaid)
                    {
                        decimal paid = _context.Payment
                            .Where(p => p.ConsumptionObjectId == obj.Id &&
                                        p.PeriodYear == year &&
                                        p.PeriodMonth == month)
                            .Sum(p => (decimal?)p.Amount) ?? 0;

                        if (paid < accrual.Amount)
                        {
                            totalDebt += (accrual.Amount - paid);
                            lastMonthWithDebt = month;
                            lastYearWithDebt = year;
                        }
                    }
                }

                if (totalDebt > 0)
                {
                    result.Add(new DebtDto
                    {
                        ConsumptionObjectId = obj.Id,
                        Address = obj.Street.Name + ", д. " + obj.HouseNumber +
                                 (obj.ApartmentNumber != null ? ", кв. " + obj.ApartmentNumber : ""),
                        DebtAmount = totalDebt,
                        PeriodMonth = lastMonthWithDebt,
                        PeriodYear = lastYearWithDebt,
                        LastPaymentDate = GetLastPaymentDate(obj.Id),
                        MonthsOverdue = CalculateMonthsOverdue(lastYearWithDebt, lastMonthWithDebt)
                    });
                }
            }

            return result.OrderByDescending(d => d.DebtAmount).ToList();
        }

        private DateTime GetLastPaymentDate(int objectId)
        {
            var lastPayment = _context.Payment
                .Where(p => p.ConsumptionObjectId == objectId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            return lastPayment?.PaymentDate ?? DateTime.MinValue;
        }

        private int CalculateMonthsOverdue(int year, int month)
        {
            if (year == 0 || month == 0) return 0;

            var currentDate = DateTime.Now;
            var debtDate = new DateTime(year, month, 1);

            return ((currentDate.Year - debtDate.Year) * 12) +
                   (currentDate.Month - debtDate.Month);
        }
    }
}
