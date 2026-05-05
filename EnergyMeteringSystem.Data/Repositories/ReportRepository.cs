using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ReportRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public ReportRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = new List<ConsumptionReportDto>();

                // Получаем все счетчики с показаниями
                var meters = _context.Meter
                    .Include(m => m.ConsumptionObject)
                    .Include(m => m.ConsumptionObject.Street)
                    .ToList();

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
                        var obj = meter.ConsumptionObject;

                        var dto = new ConsumptionReportDto
                        {
                            ObjectId = meter.ConsumptionObjectId,
                            Address = obj?.Street?.Name + ", д. " + obj?.HouseNumber +
                                     (obj?.ApartmentNumber != null ? ", кв. " + obj?.ApartmentNumber : ""),
                            MeterSerial = meter.SerialNumber,
                            StartDate = first.ReadingDate,
                            EndDate = last.ReadingDate,
                            StartValue = first.Value,
                            EndValue = last.Value,
                            Consumption = last.Value - first.Value
                        };

                        result.Add(dto);
                        System.Diagnostics.Debug.WriteLine($"Added consumption: {dto.Address} - {dto.Consumption}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"GetConsumptionReport: loaded {result.Count} records");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetConsumptionReport: {ex.Message}");
                return new List<ConsumptionReportDto>();
            }
        }

        public List<AccrualReportDto> GetAccrualReport(int year, int month)
        {
            try
            {
                var result = _context.Accrual
                    .Include(a => a.ConsumptionObject)
                    .Include(a => a.ConsumptionObject.Street)
                    .Where(a => a.PeriodYear == year && a.PeriodMonth == month)
                    .ToList()
                    .Select(a => new AccrualReportDto
                    {
                        ObjectId = a.ConsumptionObjectId,
                        Address = a.ConsumptionObject?.Street?.Name + ", д. " + a.ConsumptionObject?.HouseNumber +
                                 (a.ConsumptionObject?.ApartmentNumber != null ? ", кв. " + a.ConsumptionObject?.ApartmentNumber : ""),
                        PeriodMonth = a.PeriodMonth,
                        PeriodYear = a.PeriodYear,
                        AccrualAmount = a.Amount,
                        PaidAmount = 0, // Здесь нужно получить оплаты
                        DebtAmount = a.Amount // Пока просто сумма
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"GetAccrualReport: loaded {result.Count} records");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetAccrualReport: {ex.Message}");
                return new List<AccrualReportDto>();
            }
        }

        public List<DebtDto> GetDebtReport()
        {
            try
            {
                var result = _context.Accrual
                    .Include(a => a.ConsumptionObject)
                    .Include(a => a.ConsumptionObject.Street)
                    .Where(a => !a.IsPaid)
                    .ToList()
                    .GroupBy(a => a.ConsumptionObjectId)
                    .Select(g => new DebtDto
                    {
                        ConsumptionObjectId = g.Key,
                        Address = g.First().ConsumptionObject?.Street?.Name + ", д. " + g.First().ConsumptionObject?.HouseNumber +
                                 (g.First().ConsumptionObject?.ApartmentNumber != null ? ", кв. " + g.First().ConsumptionObject?.ApartmentNumber : ""),
                        DebtAmount = g.Sum(a => a.Amount),
                        PeriodMonth = DateTime.Now.Month,
                        PeriodYear = DateTime.Now.Year,
                        LastPaymentDate = DateTime.MinValue,
                        MonthsOverdue = 1
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"GetDebtReport: loaded {result.Count} records");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetDebtReport: {ex.Message}");
                return new List<DebtDto>();
            }
        }
    }
}