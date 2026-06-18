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
        // Константы
        private const int DEFAULT_VERIFICATION_TAKE = 500;
        private const int DEFAULT_HISTORY_TAKE = 100;

        // Синхронный (для совместимости)
        public List<MeterReadingVerificationDto> GetForVerification()
        {
            return GetForVerificationAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ метод - ОДИН ЗАПРОС!
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
        // Вспомогательный метод для получения предыдущих показаний
        private async Task<Dictionary<int, MeterReading>> GetPreviousReadingsBatchForVerificationAsync(
            List<int> meterIds, DateTime minDate, CancellationToken cancellationToken = default)
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

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с async
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

        // Синхронный Add (для совместимости)
        public void Add(MeterReadingInputDto dto)
        {
            AddAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ UpdateStatus с async
        public async Task<bool> UpdateStatusAsync(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null, CancellationToken cancellationToken = default)
        {
            var reading = await _context.MeterReading.FindAsync(cancellationToken, readingId);
            if (reading == null) return false;

            var oldStatus = reading.ReadingStatusId;

            reading.ReadingStatusId = newStatusId;  // ✅ ДОЛЖНО БЫТЬ 3
            reading.RejectionReasonId = rejectionReasonId;
            reading.Comment = comment?.Length > 500 ? comment.Substring(0, 500) : comment;

            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "MeterReading", readingId,
                new { StatusId = oldStatus },
                new { StatusId = newStatusId, rejectionReasonId, comment });

            return true;
        }

        // Синхронный UpdateStatus (для совместимости)
        public void UpdateStatus(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null)
        {
            UpdateStatusAsync(readingId, newStatusId, rejectionReasonId, comment).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetMetersByObjectId
        // ✅ ОПТИМИЗИРОВАННЫЙ GetMetersByObjectId
        public async Task<List<MeterForReadingDto>> GetMetersByObjectIdAsync(int objectId, CancellationToken cancellationToken = default)
        {
            // Один запрос со всеми данными
            var meters = await Query<Meter>()
                .Where(m => m.ConsumptionObjectId == objectId)
                .Select(m => new
                {
                    m.Id,
                    m.SerialNumber,
                    MeterTypeName = m.MeterType.Name,      // ← явное имя
                    StatusName = m.MeterStatus.Name,       // ← явное имя
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

        // ✅ ОПТИМИЗИРОВАННЫЙ GetHistoryByMeterId
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

            // Вычисляем потребление (обратный порядок)
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

        // ✅ ОПТИМИЗИРОВАННЫЙ GetHistoryByObjectId
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
        // В MeterReadingRepository.cs
        public async Task<bool> UpdateAsync(int readingId, MeterReadingInputDto dto, CancellationToken cancellationToken = default)
        {
            var reading = await _context.MeterReading.FindAsync(cancellationToken, readingId);
            if (reading == null) return false;

            var oldValue = reading.Value;
            var oldStatus = reading.ReadingStatusId;

            reading.ReadingDate = dto.ReadingDate;
            reading.Value = dto.Value;
            reading.Comment = dto.Comment;
            reading.EnteredAt = DateTime.Now;
            reading.ReadingStatusId = 1;  // ✅ СТАТУС = ВВЕДЕНО (ЧТОБЫ ПОПАЛ В ВЕРИФИКАЦИЮ)
            reading.TariffZone = dto.TariffZone;

            await _context.SaveChangesAsync(cancellationToken);

            AuditLogger.Log("UPDATE", "MeterReading", readingId,
                new { Value = oldValue, StatusId = oldStatus },
                new { Value = dto.Value, StatusId = 1 });

            return true;
        }
        
        public List<MeterReadingHistoryDto> GetHistoryByObjectId(int objectId)
        {
            return GetHistoryByObjectIdAsync(objectId).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetReadingsForPeriod
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

        // ✅ Новый метод: массовое обновление статусов
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

        // Остальные методы
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