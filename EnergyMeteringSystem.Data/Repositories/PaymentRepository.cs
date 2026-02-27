using System;
using System.Collections.Generic;
using System.Linq;
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
            Payment p = _context.Payment
                .Include("PaymentMethod")
                .Include("User")
                .FirstOrDefault(x => x.Id == id);

            return p == null
                ? null
                : new PaymentDto
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
            try
            {
                System.Diagnostics.Debug.WriteLine("PaymentRepository.Add: начало");

                Payment entity = new()
                {
                    // Id не указываем - автоинкремент
                    ConsumptionObjectId = dto.ConsumptionObjectId,
                    PaymentDate = DateTime.Now,
                    Amount = dto.Amount,
                    PaymentMethodId = dto.PaymentMethodId,
                    ReceivedByUserId = dto.ReceivedByUserId,
                    ReceiptNumber = dto.ReceiptNumber,
                    PeriodMonth = dto.PeriodMonth,
                    PeriodYear = dto.PeriodYear
                };

                _ = _context.Payment.Add(entity);
                int result = _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"PaymentRepository.Add: сохранено, результат={result}, ID={entity.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в PaymentRepository.Add: {ex.Message}");
                throw;
            }
        }

        public decimal GetTotalForPeriod(int year, int month)
        {
            return _context.Payment
                .Where(p => p.PeriodYear == year && p.PeriodMonth == month)
                .Sum(p => (decimal?)p.Amount) ?? 0;
        }

        public List<DebtDto> GetDebtors()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PaymentRepository.GetDebtors: начало");

                List<DebtDto> result = [];

                // Получаем все неоплаченные начисления
                List<Accrual> unpaidAccruals = _context.Accrual
                    .Where(a => a.IsPaid == false)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Найдено неоплаченных начислений: {unpaidAccruals.Count}");

                // Группируем по объектам
                List<IGrouping<int, Accrual>> groupedByObject = unpaidAccruals
                    .GroupBy(a => a.ConsumptionObjectId)
                    .ToList();

                foreach (IGrouping<int, Accrual> group in groupedByObject)
                {
                    int objectId = group.Key;
                    decimal totalDebt = group.Sum(a => a.Amount);

                    // Получаем информацию об объекте
                    ConsumptionObject obj = _context.ConsumptionObject
                        .Include("Street")
                        .FirstOrDefault(o => o.Id == objectId);

                    if (obj == null)
                    {
                        continue;
                    }

                    // Берем самый последний период с долгом
                    Accrual latestAccrual = group
                        .OrderByDescending(a => a.PeriodYear)
                        .ThenByDescending(a => a.PeriodMonth)
                        .First();

                    result.Add(new DebtDto
                    {
                        ConsumptionObjectId = objectId,
                        Address = obj.Street?.Name + ", д. " + obj.HouseNumber +
                                 (obj.ApartmentNumber != null ? ", кв. " + obj.ApartmentNumber : ""),
                        DebtAmount = totalDebt,
                        PeriodMonth = latestAccrual.PeriodMonth,
                        PeriodYear = latestAccrual.PeriodYear,
                        LastPaymentDate = GetLastPaymentDate(objectId),
                        MonthsOverdue = CalculateMonthsOverdue(latestAccrual.PeriodYear, latestAccrual.PeriodMonth)
                    });

                    System.Diagnostics.Debug.WriteLine($"  Добавлен должник: {obj.Id}, долг: {totalDebt}");
                }

                System.Diagnostics.Debug.WriteLine($"PaymentRepository.GetDebtors: возвращаем {result.Count} должников");
                return result.OrderByDescending(d => d.DebtAmount).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetDebtors: {ex.Message}");
                return [];
            }
        }

        private DateTime GetLastPaymentDate(int objectId)
        {
            Payment lastPayment = _context.Payment
                .Where(p => p.ConsumptionObjectId == objectId)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            return lastPayment?.PaymentDate ?? DateTime.MinValue;
        }

        private int CalculateMonthsOverdue(int year, int month)
        {
            DateTime currentDate = DateTime.Now;
            DateTime debtDate = new(year, month, 1);

            return ((currentDate.Year - debtDate.Year) * 12) +
                   (currentDate.Month - debtDate.Month);
        }
    }
}
