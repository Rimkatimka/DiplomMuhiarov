using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterReadingRepository : IMeterReadingRepository
    {
        private readonly EnergyMeteringSystemEntities _context;
        public List<MeterReadingHistoryDto> GetHistoryByMeterId(int meterId)
        {
            var readings = _context.MeterReading
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
                .ToList();

            // Вычисляем потребление (разницу с предыдущим)
            for (int i = 1; i < readings.Count; i++)
            {
                readings[i].Consumption = readings[i].Value - readings[i - 1].Value;
            }

            return readings;
        }
        public List<MeterReading> GetReadingsForPeriod(int objectId, int year, int month)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            return _context.MeterReading
                .Where(r => r.Meter.ConsumptionObjectId == objectId &&
                            r.ReadingDate >= startDate &&
                            r.ReadingDate <= endDate)
                .Include("Meter")
                .Include("ReadingStatus")
                .OrderBy(r => r.ReadingDate)
                .ToList();
        }
        public List<MeterReadingHistoryDto> GetHistoryByObjectId(int objectId)
        {
            // Получаем все показания всех счётчиков объекта
            var query = from r in _context.MeterReading
                        join m in _context.Meter on r.MeterId equals m.Id
                        where m.ConsumptionObjectId == objectId
                        orderby r.ReadingDate descending
                        select new MeterReadingHistoryDto
                        {
                            Id = r.Id,
                            ReadingDate = r.ReadingDate,
                            Value = r.Value,
                            StatusName = r.ReadingStatus.Name,
                            EnteredBy = r.User.FullName,
                            EnteredAt = r.EnteredAt
                        };

            return query.ToList();
        }
        public List<MeterReadingVerificationDto> GetForVerification()
        {
            var query = from r in _context.MeterReading
                        where r.ReadingStatusId == 1 // "Введено"
                        join m in _context.Meter on r.MeterId equals m.Id
                        join o in _context.ConsumptionObject on m.ConsumptionObjectId equals o.Id
                        join s in _context.Street on o.StreetId equals s.Id
                        join u in _context.User on r.EnteredByUserId equals u.Id
                        orderby r.ReadingDate descending
                        select new MeterReadingVerificationDto
                        {
                            Id = r.Id,
                            Address = s.Name + ", д. " + o.HouseNumber +
                                      (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                            SerialNumber = m.SerialNumber,
                            ReadingDate = r.ReadingDate,
                            Value = r.Value,
                            EnteredBy = u.FullName,
                            EnteredAt = r.EnteredAt,
                            StatusId = r.ReadingStatusId
                        };

            // Получаем предыдущие показания для каждого
            var result = query.ToList();
            foreach (var item in result)
            {
                var prev = _context.MeterReading
                    .Where(r => r.MeterId == _context.MeterReading
                        .Where(x => x.Id == item.Id).Select(x => x.MeterId).FirstOrDefault()
                        && r.ReadingDate < item.ReadingDate)
                    .OrderByDescending(r => r.ReadingDate)
                    .FirstOrDefault();

                item.PreviousValue = prev?.Value;
            }

            return result;
        }

        public void UpdateStatus(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null)
        {
            var reading = _context.MeterReading.Find(readingId);
            if (reading != null)
            {
                reading.ReadingStatusId = newStatusId;
                reading.RejectionReasonId = rejectionReasonId;
                reading.Comment = comment;
                _context.SaveChanges();
            }
        }
        public MeterReadingRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public void Add(MeterReadingInputDto dto)
        {
            var entity = new MeterReading
            {
                MeterId = dto.MeterId,
                ReadingDate = dto.ReadingDate,
                Value = dto.Value,
                EnteredByUserId = dto.EnteredByUserId,
                ReadingStatusId = dto.ReadingStatusId,
                TariffZone = dto.TariffZone,
                Comment = dto.Comment,
                EnteredAt = DateTime.Now
            };

            _context.MeterReading.Add(entity);
            _context.SaveChanges();
        }

        public List<MeterForReadingDto> GetMetersByObjectId(int objectId)
        {
            var meters = _context.Meter
                .Include("MeterType")
                .Include("MeterStatus")
                .Where(m => m.ConsumptionObjectId == objectId)
                .Select(m => new MeterForReadingDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeName = m.MeterType.Name,
                    StatusName = m.MeterStatus.Name  // ← если нужно
                })
                .ToList();

            // Добавляем последние показания
            foreach (var meter in meters)
            {
                var last = _context.MeterReading
                    .Where(r => r.MeterId == meter.Id)
                    .OrderByDescending(r => r.ReadingDate)
                    .FirstOrDefault();

                if (last != null)
                {
                    meter.LastReading = last.Value;
                    meter.LastReadingDate = last.ReadingDate;
                }
            }

            return meters;
        }

        public decimal? GetLastReading(int meterId)
        {
            return _context.MeterReading
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => (decimal?)r.Value)
                .FirstOrDefault();
        }
    }
}
