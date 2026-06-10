using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class ReportRepository : BaseRepository
    {
        public List<MonthlyConsumptionDto> GetMonthlyConsumption(int year)
        {
            return GetMonthlyConsumptionAsync(year).Result;
        }

        public async Task<List<MonthlyConsumptionDto>> GetMonthlyConsumptionAsync(int year)
        {
            var result = new List<MonthlyConsumptionDto>();

            for (int month = 1; month <= 12; month++)
            {
                DateTime startDate = new DateTime(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                var readings = await Query<MeterReading>()
                    .Where(r => r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                    .ToListAsync();

                var meterGroups = readings.GroupBy(r => r.MeterId);
                decimal monthlyConsumption = 0;

                foreach (var meterGroup in meterGroups)
                {
                    var ordered = meterGroup.OrderBy(r => r.ReadingDate).ToList();
                    if (ordered.Count >= 2)
                    {
                        var first = ordered.First();
                        var last = ordered.Last();
                        monthlyConsumption += last.Value - first.Value;
                    }
                    else if (ordered.Count == 1)
                    {
                        var prevMonth = startDate.AddMonths(-1);
                        var prevReading = await Query<MeterReading>()
                            .Where(r => r.MeterId == meterGroup.Key && r.ReadingDate >= prevMonth && r.ReadingDate < startDate)
                            .OrderByDescending(r => r.ReadingDate)
                            .FirstOrDefaultAsync();

                        if (prevReading != null)
                        {
                            monthlyConsumption += ordered.First().Value - prevReading.Value;
                        }
                    }
                }

                result.Add(new MonthlyConsumptionDto
                {
                    Year = year,
                    Month = month,
                    MonthName = GetMonthName(month),
                    Consumption = monthlyConsumption
                });
            }

            return result;
        }

        private string GetMonthName(int month)
        {
            string[] months = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            return months[month - 1];
        }

        public List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate)
        {
            return GetConsumptionReportAsync(startDate, endDate).Result;
        }

        public async Task<List<ConsumptionReportDto>> GetConsumptionReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var result = new List<ConsumptionReportDto>();
                var objects = await Query<ConsumptionObject>().ToListAsync();

                foreach (var obj in objects)
                {
                    var meters = await Query<Meter>()
                        .Where(m => m.ConsumptionObjectId == obj.Id)
                        .ToListAsync();

                    decimal totalConsumption = 0;
                    DateTime? firstReadingDate = null;
                    DateTime? lastReadingDate = null;
                    decimal firstValue = 0;
                    decimal lastValue = 0;

                    foreach (var meter in meters)
                    {
                        var readings = await Query<MeterReading>()
                            .Where(r => r.MeterId == meter.Id && r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                            .OrderBy(r => r.ReadingDate)
                            .ToListAsync();

                        if (readings.Count >= 2)
                        {
                            var first = readings.First();
                            var last = readings.Last();
                            totalConsumption += last.Value - first.Value;

                            if (firstReadingDate == null) firstReadingDate = first.ReadingDate;
                            if (lastReadingDate == null) lastReadingDate = last.ReadingDate;
                            firstValue = first.Value;
                            lastValue = last.Value;
                        }
                        else if (readings.Count == 1)
                        {
                            var single = readings.First();
                            totalConsumption += single.Value - meter.InitialReading;

                            if (firstReadingDate == null) firstReadingDate = meter.InstallationDate;
                            if (lastReadingDate == null) lastReadingDate = single.ReadingDate;
                            firstValue = meter.InitialReading;
                            lastValue = single.Value;
                        }
                    }

                    if (totalConsumption > 0)
                    {
                        var street = await Query<Street>()
                            .FirstOrDefaultAsync(s => s.Id == obj.StreetId);

                        var city = street != null ? await Query<City>()
                            .FirstOrDefaultAsync(c => c.Id == street.CityId) : null;

                        var objectType = await Query<ObjectType>()
                            .FirstOrDefaultAsync(t => t.Id == obj.ObjectTypeId);

                        string fullAddress = $"{city?.Name}, {street?.Name}, {obj.HouseNumber}";
                        if (!string.IsNullOrEmpty(obj.ApartmentNumber))
                            fullAddress += $"/{obj.ApartmentNumber}";

                        result.Add(new ConsumptionReportDto
                        {
                            ObjectId = obj.Id,
                            Address = fullAddress,
                            MeterSerial = meters.FirstOrDefault()?.SerialNumber ?? "Нет счетчика",
                            StartDate = firstReadingDate ?? startDate,
                            EndDate = lastReadingDate ?? endDate,
                            StartValue = firstValue,
                            EndValue = lastValue,
                            Consumption = totalConsumption,
                            ObjectType = objectType?.Name ?? "Не указан"
                        });
                    }
                }

                System.Diagnostics.Debug.WriteLine($"GetConsumptionReportAsync: loaded {result.Count} records");
                return result.OrderByDescending(r => r.Consumption).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GetConsumptionReportAsync: {ex.Message}");
                return new List<ConsumptionReportDto>();
            }
        }

        public class MonthlyConsumptionDto
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string MonthName { get; set; }
            public decimal Consumption { get; set; }
        }
    }
}