using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class AccrualRepository : IAccrualRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public AccrualRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<AccrualDto> GetByPeriod(int year, int month)
        {
            return _context.Accrual
                .Where(a => a.PeriodYear == year && a.PeriodMonth == month)
                .Join(_context.ConsumptionObject,
                    a => a.ConsumptionObjectId,
                    o => o.Id,
                    (a, o) => new AccrualDto
                    {
                        Id = a.Id,
                        ConsumptionObjectId = a.ConsumptionObjectId,
                        Address = o.Street.Name + ", д. " + o.HouseNumber +
                                  (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                        PeriodMonth = a.PeriodMonth,
                        PeriodYear = a.PeriodYear,
                        ConsumptionValue = a.ConsumptionValue,
                        Amount = a.Amount,
                        IsPaid = a.IsPaid
                    })
                .ToList();
        }

        public List<AccrualDto> GetByObjectId(int objectId)
        {
            return _context.Accrual
                .Where(a => a.ConsumptionObjectId == objectId)
                .OrderByDescending(a => a.PeriodYear)
                .ThenByDescending(a => a.PeriodMonth)
                .Select(a => new AccrualDto
                {
                    Id = a.Id,
                    ConsumptionObjectId = a.ConsumptionObjectId,
                    PeriodMonth = a.PeriodMonth,
                    PeriodYear = a.PeriodYear,
                    ConsumptionValue = a.ConsumptionValue,
                    Amount = a.Amount,
                    IsPaid = a.IsPaid
                })
                .ToList();
        }

        public AccrualDto GetByObjectAndPeriod(int objectId, int year, int month)
        {
            Accrual accrual = _context.Accrual
                .FirstOrDefault(a => a.ConsumptionObjectId == objectId &&
                                     a.PeriodYear == year &&
                                     a.PeriodMonth == month);

            return accrual == null
                ? null
                : new AccrualDto
                {
                    Id = accrual.Id,
                    ConsumptionObjectId = accrual.ConsumptionObjectId,
                    PeriodMonth = accrual.PeriodMonth,
                    PeriodYear = accrual.PeriodYear,
                    ConsumptionValue = accrual.ConsumptionValue,
                    Amount = accrual.Amount,
                    IsPaid = accrual.IsPaid
                };
        }

        public void Add(AccrualDto dto)
        {
            Accrual entity = new()
            {
                ConsumptionObjectId = dto.ConsumptionObjectId,
                PeriodMonth = dto.PeriodMonth,
                PeriodYear = dto.PeriodYear,
                ConsumptionValue = dto.ConsumptionValue,
                Amount = dto.Amount,
                IsPaid = dto.IsPaid,
                TariffId = 1 // По умолчанию, потом доработаем
            };

            _ = _context.Accrual.Add(entity);
            _ = _context.SaveChanges();
        }

        public void Update(AccrualDto dto)
        {
            Accrual entity = _context.Accrual.Find(dto.Id);
            if (entity != null)
            {
                entity.IsPaid = dto.IsPaid;
                _ = _context.SaveChanges();
            }
        }

        public bool Exists(int objectId, int year, int month)
        {
            return _context.Accrual.Any(a =>
                a.ConsumptionObjectId == objectId &&
                a.PeriodYear == year &&
                a.PeriodMonth == month);
        }
    }
}
