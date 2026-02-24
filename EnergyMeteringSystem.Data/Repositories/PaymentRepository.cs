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

        public List<PaymentDto> GetDebtors()
        {
            // Получаем все начисления за последний месяц
            var lastMonth = DateTime.Now.AddMonths(-1);
            var accruals = _context.Accrual
                .Where(a => a.PeriodYear == lastMonth.Year &&
                           a.PeriodMonth == lastMonth.Month &&
                           !a.IsPaid)
                .ToList();

            var result = new List<PaymentDto>();

            foreach (var a in accruals)
            {
                // Сумма оплат за этот период
                var paid = _context.Payment
                    .Where(p => p.ConsumptionObjectId == a.ConsumptionObjectId &&
                               p.PeriodYear == a.PeriodYear &&
                               p.PeriodMonth == a.PeriodMonth)
                    .Sum(p => (decimal?)p.Amount) ?? 0;

                if (paid < a.Amount)
                {
                    var obj = _context.ConsumptionObject.Find(a.ConsumptionObjectId);
                    result.Add(new PaymentDto
                    {
                        ConsumptionObjectId = a.ConsumptionObjectId,
                        Address = obj?.Street.Name + ", д. " + obj?.HouseNumber,
                        Amount = a.Amount - paid,
                        PeriodMonth = a.PeriodMonth,
                        PeriodYear = a.PeriodYear
                    });
                }
            }

            return result;
        }
    }
}
