using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterReadingRepository : IMeterReadingRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterReadingRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }
        public decimal? GetLastReading(int meterId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetLastReading: meterId={meterId}");

                decimal? lastReading = _context.MeterReading
                    .Where(r => r.MeterId == meterId)
                    .OrderByDescending(r => r.ReadingDate)
                    .Select(r => (decimal?)r.Value)
                    .FirstOrDefault();

                System.Diagnostics.Debug.WriteLine($"GetLastReading: результат={lastReading}");

                return lastReading;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetLastReading: {ex.Message}");
                return null;
            }
        }
        public DateTime? GetLastReadingDate(int meterId)
        {
            return _context.MeterReading
                .Where(r => r.MeterId == meterId)
                .OrderByDescending(r => r.ReadingDate)
                .Select(r => (DateTime?)r.ReadingDate)
                .FirstOrDefault();
        }
        public List<MeterReadingVerificationDto> GetForVerification()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GetForVerification: начало");

                var query = from r in _context.MeterReading
                            where r.ReadingStatusId == 1 // "Введено"
                            join m in _context.Meter on r.MeterId equals m.Id
                            join o in _context.ConsumptionObject on m.ConsumptionObjectId equals o.Id
                            join s in _context.Street on o.StreetId equals s.Id
                            join u in _context.User on r.EnteredByUserId equals u.Id
                            orderby r.ReadingDate descending
                            select new
                            {
                                r.Id,
                                Address = s.Name + ", д. " + o.HouseNumber +
                                          (o.ApartmentNumber != null ? ", кв. " + o.ApartmentNumber : ""),
                                r.MeterId,
                                m.SerialNumber,
                                r.ReadingDate,
                                r.Value,
                                EnteredBy = u.FullName,
                                r.EnteredAt,
                                r.ReadingStatusId
                            };

                var result = query.ToList();
                System.Diagnostics.Debug.WriteLine($"GetForVerification: загружено {result.Count} записей");

                List<MeterReadingVerificationDto> verificationList = [];

                foreach (var item in result)
                {
                    // Получаем предыдущее показание для этого счетчика
                    MeterReading previous = _context.MeterReading
                        .Where(r => r.MeterId == item.MeterId && r.ReadingDate < item.ReadingDate)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefault();

                    verificationList.Add(new MeterReadingVerificationDto
                    {
                        Address = item.Address,
                        SerialNumber = item.SerialNumber,
                        ReadingDate = item.ReadingDate,
                        Value = item.Value,
                        PreviousValue = previous?.Value,
                        EnteredBy = item.EnteredBy,
                        EnteredAt = item.EnteredAt,
                        StatusId = item.ReadingStatusId,
                        StatusName = GetStatusName(item.ReadingStatusId),
                        IsSelected = false
                    });
                }

                return verificationList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetForVerification: {ex.Message}");
                return [];
            }
        }
        public void UpdateStatus(int readingId, int newStatusId, int? rejectionReasonId = null, string comment = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatus: readingId={readingId}, newStatusId={newStatusId}");

                MeterReading reading = _context.MeterReading.Find(readingId);
                if (reading != null)
                {
                    reading.ReadingStatusId = newStatusId;
                    reading.RejectionReasonId = rejectionReasonId;
                    reading.Comment = comment;

                    _ = _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("UpdateStatus: успешно");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateStatus: {ex.Message}");
                throw;
            }
        }

        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Введено",
                2 => "Подтверждено",
                3 => "Отклонено",
                _ => "Неизвестно",
            };
        }

        public void Add(MeterReadingInputDto dto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== MeterReadingRepository.Add ===");

                // Проверяем, есть ли уже показания за эту дату
                MeterReading existingReading = _context.MeterReading
                    .FirstOrDefault(r => r.MeterId == dto.MeterId &&
                                         r.ReadingDate == dto.ReadingDate &&
                                         r.TariffZone == dto.TariffZone);

                if (existingReading != null)
                {
                    System.Diagnostics.Debug.WriteLine("Показания за эту дату уже существуют");
                    throw new InvalidOperationException("Показания за эту дату уже были введены. Для корректировки используйте редактирование.");
                }

                MeterReading entity = new()
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

                _ = _context.MeterReading.Add(entity);
                _ = _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine("Сохранено успешно");
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("UQ_MeterReading_Meter_Date"))
                {
                    throw new InvalidOperationException("Показания за эту дату уже были введены. Для корректировки используйте редактирование.", ex);
                }
                throw;
            }
        }
        public List<MeterReadingDto> GetReadingsForPeriod(int objectId, int year, int month)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetReadingsForPeriod: objectId={objectId}, year={year}, month={month}");

                DateTime startDate = new(year, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                // Простой запрос без сложных Include
                List<MeterReading> readings = _context.MeterReading
                    .Where(r => r.Meter.ConsumptionObjectId == objectId &&
                                r.ReadingDate >= startDate &&
                                r.ReadingDate <= endDate)
                    .ToList();  // Сначала получаем данные

                // Затем формируем DTO в памяти
                List<MeterReadingDto> result = [];

                foreach (MeterReading r in readings)
                {
                    // Загружаем связанные данные вручную при необходимости
                    _context.Entry(r).Reference(x => x.Meter).Load();
                    _context.Entry(r).Reference(x => x.ReadingStatus).Load();
                    _context.Entry(r).Reference(x => x.User).Load();

                    result.Add(new MeterReadingDto
                    {
                        Id = r.Id,
                        MeterId = r.MeterId,
                        ReadingDate = r.ReadingDate,
                        Value = r.Value,
                        ReadingStatusId = r.ReadingStatusId,
                        StatusName = r.ReadingStatus?.Name,
                        EnteredBy = r.User?.FullName,
                        EnteredAt = r.EnteredAt
                    });
                }

                System.Diagnostics.Debug.WriteLine($"GetReadingsForPeriod: найдено {result.Count} показаний");
                return result.OrderBy(r => r.ReadingDate).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetReadingsForPeriod: {ex.Message}");
                return [];
            }
        }
        public List<MeterReadingHistoryDto> GetHistoryByMeterId(int meterId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetHistoryByMeterId: meterId={meterId}");

                var readings = _context.MeterReading
                    .Where(r => r.MeterId == meterId)
                    .OrderBy(r => r.ReadingDate)
                    .Select(r => new
                    {
                        r.Id,
                        r.ReadingDate,
                        r.Value,
                        r.ReadingStatusId,
                        StatusName = r.ReadingStatus.Name,
                        EnteredBy = r.User.FullName,
                        r.EnteredAt
                    })
                    .ToList();

                List<MeterReadingHistoryDto> result = [];

                for (int i = 0; i < readings.Count; i++)
                {
                    var item = readings[i];
                    decimal? consumption = null;

                    if (i > 0)
                    {
                        consumption = item.Value - readings[i - 1].Value;
                    }

                    result.Add(new MeterReadingHistoryDto
                    {
                        Id = item.Id,
                        ReadingDate = item.ReadingDate,
                        Value = item.Value,
                        Consumption = consumption,
                        StatusName = item.StatusName,
                        EnteredBy = item.EnteredBy,
                        EnteredAt = item.EnteredAt
                    });
                }

                System.Diagnostics.Debug.WriteLine($"GetHistoryByMeterId: загружено {result.Count} записей");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetHistoryByMeterId: {ex.Message}");
                return [];
            }
        }
        public MeterReadingDto GetByMeterAndDate(int meterId, DateTime readingDate, int tariffZone = 1)
        {
            var reading = _context.MeterReading
                .FirstOrDefault(r => r.MeterId == meterId &&
                                      r.ReadingDate == readingDate &&
                                      r.TariffZone == tariffZone);

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
        public List<MeterReadingHistoryDto> GetHistoryByObjectId(int objectId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetHistoryByObjectId: objectId={objectId}");

                IQueryable<MeterReadingHistoryDto> query = from r in _context.MeterReading
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

                List<MeterReadingHistoryDto> result = query.ToList();
                System.Diagnostics.Debug.WriteLine($"GetHistoryByObjectId: загружено {result.Count} записей");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetHistoryByObjectId: {ex.Message}");
                return [];
            }
        }

        public List<MeterForReadingDto> GetMetersByObjectId(int objectId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetMetersByObjectId: objectId={objectId}");

                List<Meter> meters = _context.Meter
                    .Where(m => m.ConsumptionObjectId == objectId)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"GetMetersByObjectId: найдено {meters.Count} счетчиков");

                List<MeterForReadingDto> result = [];

                foreach (Meter m in meters)
                {
                    MeterReading lastReading = _context.MeterReading
                        .Where(r => r.MeterId == m.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .FirstOrDefault();

                    result.Add(new MeterForReadingDto
                    {
                        Id = m.Id,
                        SerialNumber = m.SerialNumber,
                        MeterTypeName = m.MeterType?.Name ?? "Неизвестно",
                        LastReading = lastReading?.Value,
                        LastReadingDate = lastReading?.ReadingDate
                    });

                    System.Diagnostics.Debug.WriteLine($"  - Счетчик: {m.SerialNumber}, последнее показание: {lastReading?.Value}");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetMetersByObjectId: {ex.Message}");
                return [];
            }
        }
    }
}
