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
    public class ReportRepository : IReportRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ReportRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate)
        {
            var result = new List<ConsumptionReportDto>();
            var meters = _context.Meter.ToList();

            foreach (var meter in meters)
            {
                var readings = _context.MeterReading
                    .Where(r => r.MeterId == meter.Id &&
                                r.ReadingDate >= startDate &&
                                r.ReadingDate <= endDate)
                    .OrderBy(r => r.ReadingDate)
                    .ToList();

                if (readings.Count >= 2)
                {
                    var first = readings.First();
                    var last = readings.Last();
                    var obj = _context.ConsumptionObject.Find(meter.ConsumptionObjectId);

                    result.Add(new ConsumptionReportDto
                    {
                        ObjectId = meter.ConsumptionObjectId,
                        Address = obj.Street.Name + ", д. " + obj.HouseNumber +
                                 (obj.ApartmentNumber != null ? ", кв. " + obj.ApartmentNumber : ""),
                        MeterSerial = meter.SerialNumber,
                        StartDate = first.ReadingDate,
                        EndDate = last.ReadingDate,
                        StartValue = first.Value,
                        EndValue = last.Value,
                        Consumption = last.Value - first.Value
                    });
                }
            }

            return result;
        }

        public List<AccrualReportDto> GetAccrualReport(int year, int month)
        {
            var result = new List<AccrualReportDto>();
            var objects = _context.ConsumptionObject.ToList();

            foreach (var obj in objects)
            {
                var accrual = _context.Accrual
                    .FirstOrDefault(a => a.ConsumptionObjectId == obj.Id &&
                                         a.PeriodYear == year &&
                                         a.PeriodMonth == month);

                if (accrual != null)
                {
                    decimal paid = _context.Payment
                        .Where(p => p.ConsumptionObjectId == obj.Id &&
                                    p.PeriodYear == year &&
                                    p.PeriodMonth == month)
                        .Sum(p => (decimal?)p.Amount) ?? 0;

                    result.Add(new AccrualReportDto
                    {
                        ObjectId = obj.Id,
                        Address = obj.Street.Name + ", д. " + obj.HouseNumber +
                                 (obj.ApartmentNumber != null ? ", кв. " + obj.ApartmentNumber : ""),
                        PeriodMonth = month,
                        PeriodYear = year,
                        AccrualAmount = accrual.Amount,
                        PaidAmount = paid,
                        DebtAmount = accrual.Amount - paid
                    });
                }
            }

            return result;
        }

        public List<DebtDto> GetDebtReport()
        {
            // Используем уже готовый метод из PaymentRepository
            var paymentRepo = new PaymentRepository();
            return paymentRepo.GetDebtors();
        }
    }
}
