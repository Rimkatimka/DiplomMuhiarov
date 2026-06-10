using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterReadingRepository : BaseRepository, IMeterReadingRepository
    {
        // Синхронный (для совместимости)
        public List<MeterReadingVerificationDto> GetForVerification()
        {
            return GetForVerificationAsync().Result;
        }

        // ✅ АСИНХРОННЫЙ
        public async Task<List<MeterReadingVerificationDto>> GetForVerificationAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GetForVerificationAsync: начало");

                var result = new List<MeterReadingVerificationDto>();

                var readings = await Query<MeterReading>()
                    .Where(r => r.ReadingStatusId == 1)
                    .OrderByDescending(r => r.ReadingDate)
                    .ToListAsync();

                foreach (var reading in readings)
                {
                    var meter = await Query<Meter>()
                        .FirstOrDefaultAsync(m => m.Id == reading.MeterId);
                    if (meter == null) continue;

                    var obj = await Query<ConsumptionObject>()
                        .FirstOrDefaultAsync(o => o.Id == meter.ConsumptionObjectId);
                    if (obj == null) continue;

                    var street = await Query<Street>()
                        .FirstOrDefaultAsync(s => s.Id == obj.StreetId);
                    if (street == null) continue;

                    var city = await Query<City>()
                        .FirstOrDefaultAsync(c => c.Id == street.CityId);
                    if (city == null) continue;

                    var region = await Query<Region>()
                        .FirstOrDefaultAsync(r => r.Id == city.RegionId);
                    if (region == null) continue;

                    var user = await Query<User>()
                        .FirstOrDefaultAsync(u => u.Id == reading.EnteredByUserId);
                    var status = await Query<ReadingStatus>()
                        .FirstOrDefaultAsync(rs => rs.Id == reading.ReadingStatusId);

                    decimal? previousValue = null;
                    var previous = await Query<MeterReading>()
                        .Where(r => r.MeterId == reading.MeterId && r.ReadingDate < reading.ReadingDate)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefaultAsync();
                    if (previous != null)
                    {
                        previousValue = previous.Value;
                    }

                    string fullAddress = "";
                    if (region != null && !string.IsNullOrEmpty(region.Name))
                        fullAddress += region.Name + ", ";
                    if (city != null && !string.IsNullOrEmpty(city.Name))
                        fullAddress += city.Name + ", ";
                    if (street != null && !string.IsNullOrEmpty(street.Name))
                        fullAddress += street.Name + ", ";
                    fullAddress += "д. " + obj.HouseNumber;
                    if (!string.IsNullOrEmpty(obj.ApartmentNumber))
                        fullAddress += ", кв. " + obj.ApartmentNumber;

                    result.Add(new MeterReadingVerificationDto
                    {
                        Id = reading.Id,
                        Address = fullAddress,
                        SerialNumber = meter.SerialNumber ?? "Нет номера",
                        ReadingDate = reading.ReadingDate,
                        Value = reading.Value,
                        PreviousValue = previousValue,
                        EnteredBy = user?.FullName ?? "Неизвестно",
                        EnteredAt = reading.EnteredAt,
                        StatusId = reading.ReadingStatusId,
                        StatusName = status?.Name ?? "Введено",
                        IsSelected = false
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка: {ex.Message}");
                return new List<MeterReadingVerificationDto>();
            }
        }

        public async Task<decimal?> GetLastReadingAsync(int meterId)
        {
            return await Query<MeterReading>()
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => (decimal?)r.Value)
                .FirstOrDefaultAsync();
        }

        public decimal? GetLastReading(int meterId)
        {
            return GetLastReadingAsync(meterId).Result;
        }

        public DateTime? GetLastReadingDate(int meterId)
        {
            return GetLastReadingDateAsync(meterId).Result;
        }

        public async Task<DateTime?> GetLastReadingDateAsync(int meterId)
        {
            return await Query<MeterReading>()
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => (DateTime?)r.ReadingDate)
                .FirstOrDefaultAsync();
        }

        public void Add(MeterReadingInputDto dto)
        {
            var entity = new MeterReading
            {
                MeterId = dto.MeterId,
                ReadingDate = dto.ReadingDate,
                Value = dto.Value,
                EnteredAt = DateTime.Now,
                EnteredByUserId = dto.EnteredByUserId,
                ReadingStatusId = dto.ReadingStatusId,
                RejectionReasonId = dto.RejectionReasonId,
                Comment = dto.Comment,
                TariffZone = dto.TariffZone
            };
            _context.MeterReading.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "MeterReading", entity.Id, null,
                new { dto.MeterId, dto.Value, dto.ReadingDate, dto.ReadingStatusId });
        }

        public void UpdateStatus(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null)
        {
            var reading = _context.MeterReading.Find(readingId);
            if (reading != null)
            {
                var oldStatus = reading.ReadingStatusId;

                reading.ReadingStatusId = newStatusId;
                reading.RejectionReasonId = rejectionReasonId;
                reading.Comment = comment;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "MeterReading", readingId,
                    new { StatusId = oldStatus },
                    new { StatusId = newStatusId, rejectionReasonId, comment });
            }
        }

        public List<MeterForReadingDto> GetMetersByObjectId(int objectId)
        {
            return GetMetersByObjectIdAsync(objectId).Result;
        }

        public async Task<List<MeterForReadingDto>> GetMetersByObjectIdAsync(int objectId)
        {
            var meters = await Query<Meter>()
                .Where(m => m.ConsumptionObjectId == objectId)
                .ToListAsync();

            var result = new List<MeterForReadingDto>();

            foreach (var m in meters)
            {
                var lastReading = await Query<MeterReading>()
                    .Where(r => r.MeterId == m.Id)
                    .OrderByDescending(r => r.ReadingDate)
                    .Select(r => (decimal?)r.Value)
                    .FirstOrDefaultAsync();

                result.Add(new MeterForReadingDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeName = m.MeterType?.Name,
                    LastReading = lastReading,
                    LastReadingDate = null,
                    StatusName = m.MeterStatus?.Name
                });
            }
            return result;
        }

        public List<MeterReadingHistoryDto> GetHistoryByMeterId(int meterId)
        {
            return GetHistoryByMeterIdAsync(meterId).Result;
        }

        public async Task<List<MeterReadingHistoryDto>> GetHistoryByMeterIdAsync(int meterId)
        {
            var readings = await Query<MeterReading>()
                .Where(r => r.MeterId == meterId)
                .OrderBy(r => r.ReadingDate)
                .Select(r => new MeterReadingHistoryDto
                {
                    Id = r.Id,
                    ReadingDate = r.ReadingDate,
                    Value = r.Value,
                    StatusName = r.ReadingStatus.Name,
                    EnteredBy = r.User.FullName,
                    EnteredAt = r.EnteredAt
                })
                .ToListAsync();

            for (int i = 0; i < readings.Count; i++)
            {
                if (i > 0)
                {
                    readings[i].Consumption = readings[i].Value - readings[i - 1].Value;
                }
            }

            return readings;
        }

        // Остальные методы по аналогии...
        public List<MeterReadingHistoryDto> GetHistoryByObjectId(int objectId)
        {
            return GetHistoryByObjectIdAsync(objectId).Result;
        }

        public async Task<List<MeterReadingHistoryDto>> GetHistoryByObjectIdAsync(int objectId)
        {
            var readings = await (from r in Query<MeterReading>()
                                  join m in Query<Meter>() on r.MeterId equals m.Id
                                  where m.ConsumptionObjectId == objectId
                                  orderby r.ReadingDate descending
                                  select r).ToListAsync();

            return readings.Select(r => new MeterReadingHistoryDto
            {
                Id = r.Id,
                ReadingDate = r.ReadingDate,
                Value = r.Value,
                StatusName = r.ReadingStatus?.Name,
                EnteredBy = r.User?.FullName,
                EnteredAt = r.EnteredAt
            }).ToList();
        }

        public List<MeterReadingDto> GetReadingsForPeriod(int objectId, int year, int month)
        {
            return GetReadingsForPeriodAsync(objectId, year, month).Result;
        }

        public async Task<List<MeterReadingDto>> GetReadingsForPeriodAsync(int objectId, int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var readings = await Query<MeterReading>()
                .Where(r => r.Meter.ConsumptionObjectId == objectId && r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .ToListAsync();

            return readings.Select(r => new MeterReadingDto
            {
                Id = r.Id,
                MeterId = r.MeterId,
                ReadingDate = r.ReadingDate,
                Value = r.Value,
                ReadingStatusId = r.ReadingStatusId,
                StatusName = r.ReadingStatus?.Name,
                EnteredBy = r.User?.FullName,
                EnteredAt = r.EnteredAt
            }).ToList();
        }

        public MeterReadingDto GetByMeterAndDate(int meterId, DateTime readingDate, int tariffZone = 1)
        {
            return GetByMeterAndDateAsync(meterId, readingDate, tariffZone).Result;
        }

        public async Task<MeterReadingDto> GetByMeterAndDateAsync(int meterId, DateTime readingDate, int tariffZone = 1)
        {
            var reading = await Query<MeterReading>()
                .FirstOrDefaultAsync(r => r.MeterId == meterId && r.ReadingDate == readingDate && r.TariffZone == tariffZone);

            if (reading == null) return null;

            return new MeterReadingDto
            {
                Id = reading.Id,
                MeterId = reading.MeterId,
                ReadingDate = reading.ReadingDate,
                Value = reading.Value,
                ReadingStatusId = reading.ReadingStatusId,
                EnteredAt = reading.EnteredAt
            };
        }
    }
}