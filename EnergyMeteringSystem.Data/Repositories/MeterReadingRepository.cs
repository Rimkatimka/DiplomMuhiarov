using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterReadingRepository : BaseRepository, IMeterReadingRepository
    {
        private const int DEFAULT_VERIFICATION_TAKE = 500;
        private const int DEFAULT_HISTORY_TAKE = 100;

        public List<MeterReadingVerificationDto> GetForVerification()
        {
            return GetForVerificationAsync().Result;
        }

        public async Task<List<MeterReadingVerificationDto>> GetForVerificationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GetForVerificationAsync: запрос к БД");

                var query = await (from r in Query<MeterReading>()
                                   join m in Query<Meter>() on r.MeterId equals m.Id
                                   join o in Query<ConsumptionObject>() on m.ConsumptionObjectId equals o.Id
                                   join s in Query<Street>() on o.StreetId equals s.Id
                                   join c in Query<City>() on s.CityId equals c.Id
                                   join reg in Query<Region>() on c.RegionId equals reg.Id
                                   join u in Query<User>() on r.EnteredByUserId equals u.Id
                                   join rs in Query<ReadingStatus>() on r.ReadingStatusId equals rs.Id
                                   where r.ReadingStatusId == 1
                                   orderby r.EnteredAt descending
                                   select new
                                   {
                                       r.Id,
                                       r.MeterId,
                                       r.ReadingDate,
                                       r.Value,
                                       r.EnteredAt,
                                       r.ReadingStatusId,
                                       MeterSerial = m.SerialNumber,
                                       HouseNumber = o.HouseNumber,
                                       ApartmentNumber = o.ApartmentNumber,
                                       StreetName = s.Name,
                                       CityName = c.Name,
                                       RegionName = reg.Name,
                                       EnteredByName = u.FullName,
                                       StatusName = rs.Name
                                   })
                                   .Take(DEFAULT_VERIFICATION_TAKE)
                                   .ToListAsync(cancellationToken);

                System.Diagnostics.Debug.WriteLine($"GetForVerificationAsync: загружено {query.Count} записей");

                if (!query.Any()) return new List<MeterReadingVerificationDto>();

                var meterIds = query.Select(x => x.MeterId).Distinct().ToList();

                var lastReadingsBeforeDate = await GetPreviousReadingsBatchForVerificationAsync(
                    meterIds,
                    query.Min(x => x.ReadingDate),
                    cancellationToken);

                var result = new List<MeterReadingVerificationDto>();

                foreach (var item in query)
                {
                    string fullAddress = $"{item.RegionName}, {item.CityName}, {item.StreetName}, д. {item.HouseNumber}";
                    if (!string.IsNullOrEmpty(item.ApartmentNumber))
                        fullAddress += $", кв. {item.ApartmentNumber}";

                    decimal? previousValue = null;
                    if (lastReadingsBeforeDate.TryGetValue(item.MeterId, out var prevReading))
                    {
                        previousValue = prevReading.Value;
                    }

                    result.Add(new MeterReadingVerificationDto
                    {
                        Id = item.Id,
                        Address = fullAddress,
                        SerialNumber = item.MeterSerial ?? "Нет номера",
                        ReadingDate = item.ReadingDate,
                        Value = item.Value,
                        PreviousValue = previousValue,
                        EnteredBy = item.EnteredByName ?? "Неизвестно",
                        EnteredAt = item.EnteredAt,
                        StatusId = item.ReadingStatusId,
                        StatusName = item.StatusName ?? "Введено",
                        IsSelected = false
                    });
                }

                System.Diagnostics.Debug.WriteLine($"GetForVerificationAsync: возвращено {result.Count} записей");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка GetForVerificationAsync: {ex.Message}");
                return new List<MeterReadingVerificationDto>();
            }
        }

        private async Task<Dictionary<int, MeterReading>> GetPreviousReadingsBatchForVerificationAsync(
            List<int> meterIds,
            DateTime minDate,
            CancellationToken cancellationToken = default)
        {
            if (meterIds == null || !meterIds.Any())
                return new Dictionary<int, MeterReading>();

            var previousReadings = await Query<MeterReading>()
                .Where(r => meterIds.Contains(r.MeterId) && r.ReadingDate < minDate)
                .GroupBy(r => r.MeterId)
                .Select(g => g.OrderByDescending(r => r.ReadingDate).FirstOrDefault())
                .ToDictionaryAsync(r => r.MeterId, r => r, cancellationToken);

            return previousReadings;
        }

        public async Task<int> AddAsync(MeterReadingInputDto dto, CancellationToken cancellationToken = default)
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
                Comment = dto.Comment?.Length > 500 ? dto.Comment.Substring(0, 500) : dto.Comment,
                TariffZone = dto.TariffZone
            };
            _context.MeterReading.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("INSERT", "MeterReading", entity.Id, null,
                new { dto.MeterId, dto.Value, dto.ReadingDate, dto.ReadingStatusId });

            return entity.Id;
        }

        public void Add(MeterReadingInputDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateStatusAsync(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateStatusAsync НАЧАЛО");
            System.Diagnostics.Debug.WriteLine($"  readingId: {readingId}");
            System.Diagnostics.Debug.WriteLine($"  newStatusId: {newStatusId}");

            try
            {
                var reading = await _context.MeterReading.FindAsync(cancellationToken, readingId);
                if (reading == null)
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ Показание с Id={readingId} не найдено!");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"  Текущий статус: {reading.ReadingStatusId}");

                var oldStatus = reading.ReadingStatusId;

                reading.ReadingStatusId = newStatusId;
                reading.RejectionReasonId = rejectionReasonId;
                reading.Comment = comment?.Length > 500 ? comment.Substring(0, 500) : comment;

                await _context.SaveChangesAsync(cancellationToken);

                System.Diagnostics.Debug.WriteLine($"  ✅ Статус изменен с {oldStatus} на {newStatusId}");

                AuditLogger.Log("UPDATE", "MeterReading", readingId,
                    new { StatusId = oldStatus },
                    new { StatusId = newStatusId, rejectionReasonId, comment });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  ❌ ОШИБКА: {ex.Message}");
                return false;
            }
        }

        public void UpdateStatus(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null)
        {
            UpdateStatusAsync(readingId, newStatusId, rejectionReasonId, comment).Wait();
        }

        public async Task<bool> UpdateAsync(int readingId, MeterReadingInputDto dto, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateAsync НАЧАЛО");

            try
            {
                var reading = await _context.MeterReading.FindAsync(cancellationToken, readingId);
                if (reading == null)
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ Показание с Id={readingId} не найдено!");
                    return false;
                }

                var oldValue = reading.Value;
                var oldStatus = reading.ReadingStatusId;

                reading.ReadingDate = dto.ReadingDate;
                reading.Value = dto.Value;
                reading.Comment = dto.Comment;
                reading.EnteredAt = DateTime.Now;
                reading.ReadingStatusId = 1;
                reading.TariffZone = dto.TariffZone;

                await _context.SaveChangesAsync(cancellationToken);

                System.Diagnostics.Debug.WriteLine($"  ✅ Обновлено! Id={readingId}, Value={oldValue}->{dto.Value}, Status={oldStatus}->1");

                AuditLogger.Log("UPDATE", "MeterReading", readingId,
                    new { Value = oldValue, StatusId = oldStatus },
                    new { Value = dto.Value, StatusId = 1 });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"  ❌ ОШИБКА: {ex.Message}");
                return false;
            }
        }

        public async Task<List<MeterForReadingDto>> GetMetersByObjectIdAsync(int objectId, CancellationToken cancellationToken = default)
        {
            var meters = await Query<Meter>()
                .Where(m => m.ConsumptionObjectId == objectId)
                .Select(m => new
                {
                    m.Id,
                    m.SerialNumber,
                    MeterTypeName = m.MeterType.Name,
                    StatusName = m.MeterStatus.Name,
                    m.InitialReading,
                    m.InstallationDate,
                    LastReading = m.MeterReading
                        .OrderByDescending(r => r.ReadingDate)
                        .Select(r => (decimal?)r.Value)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return meters.Select(m => new MeterForReadingDto
            {
                Id = m.Id,
                SerialNumber = m.SerialNumber,
                MeterTypeName = m.MeterTypeName,
                StatusName = m.StatusName,
                LastReading = m.LastReading,
                InitialReading = m.InitialReading,
                InstallationDate = m.InstallationDate
            }).ToList();
        }

        public List<MeterForReadingDto> GetMetersByObjectId(int objectId)
        {
            return GetMetersByObjectIdAsync(objectId).Result;
        }

        public async Task<List<MeterReadingHistoryDto>> GetHistoryByMeterIdAsync(int meterId, int take = DEFAULT_HISTORY_TAKE, CancellationToken cancellationToken = default)
        {
            var readings = await Query<MeterReading>()
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Take(take)
                .Select(r => new MeterReadingHistoryDto
                {
                    Id = r.Id,
                    ReadingDate = r.ReadingDate,
                    Value = r.Value,
                    StatusName = r.ReadingStatus.Name,
                    EnteredBy = r.User.FullName,
                    EnteredAt = r.EnteredAt
                })
                .ToListAsync(cancellationToken);

            for (int i = readings.Count - 1; i > 0; i--)
            {
                readings[i - 1].Consumption = readings[i - 1].Value - readings[i].Value;
            }

            return readings;
        }

        public List<MeterReadingHistoryDto> GetHistoryByMeterId(int meterId)
        {
            return GetHistoryByMeterIdAsync(meterId).Result;
        }

        public async Task<List<MeterReadingHistoryDto>> GetHistoryByObjectIdAsync(int objectId, int take = DEFAULT_HISTORY_TAKE, CancellationToken cancellationToken = default)
        {
            var readings = await Query<MeterReading>()
                .Where(r => r.Meter.ConsumptionObjectId == objectId)
                .OrderByDescending(r => r.ReadingDate)
                .Take(take)
                .Select(r => new MeterReadingHistoryDto
                {
                    Id = r.Id,
                    ReadingDate = r.ReadingDate,
                    Value = r.Value,
                    StatusName = r.ReadingStatus.Name,
                    EnteredBy = r.User.FullName,
                    EnteredAt = r.EnteredAt
                })
                .ToListAsync(cancellationToken);

            return readings;
        }

        public List<MeterReadingHistoryDto> GetHistoryByObjectId(int objectId)
        {
            return GetHistoryByObjectIdAsync(objectId).Result;
        }

        public async Task<List<MeterReadingDto>> GetReadingsForPeriodAsync(int objectId, int year, int month, CancellationToken cancellationToken = default)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            var readings = await Query<MeterReading>()
                .Where(r => r.Meter.ConsumptionObjectId == objectId && r.ReadingDate >= startDate && r.ReadingDate <= endDate)
                .Select(r => new MeterReadingDto
                {
                    Id = r.Id,
                    MeterId = r.MeterId,
                    ReadingDate = r.ReadingDate,
                    Value = r.Value,
                    ReadingStatusId = r.ReadingStatusId,
                    StatusName = r.ReadingStatus.Name,
                    EnteredBy = r.User.FullName,
                    EnteredAt = r.EnteredAt
                })
                .ToListAsync(cancellationToken);

            return readings;
        }

        public List<MeterReadingDto> GetReadingsForPeriod(int objectId, int year, int month)
        {
            return GetReadingsForPeriodAsync(objectId, year, month).Result;
        }

        public async Task<int> BatchUpdateStatusAsync(List<int> readingIds, int newStatusId, int? rejectionReasonId = null, string comment = null, CancellationToken cancellationToken = default)
        {
            var readings = await _context.MeterReading
                .Where(r => readingIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            foreach (var reading in readings)
            {
                reading.ReadingStatusId = newStatusId;
                reading.RejectionReasonId = rejectionReasonId;
                if (comment != null)
                    reading.Comment = comment.Length > 500 ? comment.Substring(0, 500) : comment;
            }

            var count = await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("BATCH_UPDATE", "MeterReading", 0, null,
                new { Count = count, NewStatusId = newStatusId });

            return count;
        }

        public async Task<decimal?> GetLastReadingAsync(int meterId, CancellationToken cancellationToken = default)
        {
            return await Query<MeterReading>()
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => (decimal?)r.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public decimal? GetLastReading(int meterId)
        {
            return GetLastReadingAsync(meterId).Result;
        }

        public async Task<DateTime?> GetLastReadingDateAsync(int meterId, CancellationToken cancellationToken = default)
        {
            return await Query<MeterReading>()
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => (DateTime?)r.ReadingDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public DateTime? GetLastReadingDate(int meterId)
        {
            return GetLastReadingDateAsync(meterId).Result;
        }

        public async Task<MeterReadingDto> GetByMeterAndDateAsync(int meterId, DateTime readingDate, int tariffZone = 1, CancellationToken cancellationToken = default)
        {
            var reading = await Query<MeterReading>()
                .FirstOrDefaultAsync(r => r.MeterId == meterId && r.ReadingDate == readingDate && r.TariffZone == tariffZone, cancellationToken);

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

        public MeterReadingDto GetByMeterAndDate(int meterId, DateTime readingDate, int tariffZone = 1)
        {
            return GetByMeterAndDateAsync(meterId, readingDate, tariffZone).Result;
        }
    }
}