using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterRepository : BaseRepository, IMeterRepository
    {
        // Синхронные методы (для совместимости)
        public List<MeterDto> GetByObjectId(int objectId)
        {
            return GetByObjectIdAsync(objectId).Result;
        }

        public async Task<List<MeterDto>> GetByObjectIdAsync(int objectId)
        {
            try
            {
                return await Query<Meter>()
                    .Where(m => m.ConsumptionObjectId == objectId)
                    .Select(m => new MeterDto
                    {
                        Id = m.Id,
                        SerialNumber = m.SerialNumber,
                        MeterTypeId = m.MeterTypeId,
                        MeterTypeName = m.MeterType.Name,
                        StatusId = m.MeterStatusId,
                        StatusName = m.MeterStatus.Name,
                        InstallationDate = m.InstallationDate,
                        InitialReading = m.InitialReading,
                        ConsumptionObjectId = m.ConsumptionObjectId,
                        ServiceLifeYears = m.ServiceLifeYears,
                        RemovalDate = m.RemovalDate,
                        LastVerificationDate = m.VerificationDate,
                        NextVerificationDate = m.NextVerificationDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetByObjectIdAsync: {ex.Message}");
                return new List<MeterDto>();
            }
        }

        public List<MeterDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<MeterDto>> GetAllAsync()
        {
            try
            {
                var meters = await Query<Meter>()
                    .Include(m => m.MeterType)
                    .Include(m => m.MeterStatus)
                    .ToListAsync();

                return meters.Select(m => new MeterDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeId = m.MeterTypeId,
                    MeterTypeName = m.MeterType?.Name ?? "Неизвестно",
                    StatusId = m.MeterStatusId,
                    StatusName = m.MeterStatus?.Name ?? "Неизвестно",
                    InstallationDate = m.InstallationDate,
                    LastVerificationDate = m.VerificationDate,
                    NextVerificationDate = m.NextVerificationDate,
                    InitialReading = m.InitialReading,
                    ServiceLifeYears = m.ServiceLifeYears,
                    ConsumptionObjectId = m.ConsumptionObjectId
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetAllAsync: {ex.Message}");
                return new List<MeterDto>();
            }
        }

        public MeterDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<MeterDto> GetByIdAsync(int id)
        {
            var meter = await Query<Meter>()
                .Include(m => m.MeterType)
                .Include(m => m.MeterStatus)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meter == null) return null;

            return new MeterDto
            {
                Id = meter.Id,
                SerialNumber = meter.SerialNumber,
                MeterTypeId = meter.MeterTypeId,
                MeterTypeName = meter.MeterType?.Name,
                StatusId = meter.MeterStatusId,
                StatusName = meter.MeterStatus?.Name,
                InstallationDate = meter.InstallationDate,
                LastVerificationDate = meter.VerificationDate,
                NextVerificationDate = meter.NextVerificationDate,
                InitialReading = meter.InitialReading,
                ConsumptionObjectId = meter.ConsumptionObjectId,
                ServiceLifeYears = meter.ServiceLifeYears
            };
        }

        public void Add(MeterDto dto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var entity = new Meter
                    {
                        SerialNumber = dto.SerialNumber,
                        MeterTypeId = dto.MeterTypeId,
                        ConsumptionObjectId = dto.ConsumptionObjectId,
                        InstallationDate = dto.InstallationDate,
                        InitialReading = dto.InitialReading,
                        VerificationDate = dto.LastVerificationDate,
                        NextVerificationDate = dto.NextVerificationDate,
                        MeterStatusId = dto.StatusId,
                        ServiceLifeYears = dto.ServiceLifeYears
                    };

                    _context.Meter.Add(entity);
                    _context.SaveChanges();

                    if (dto.InitialReading > 0)
                    {
                        var initialReading = new MeterReading
                        {
                            MeterId = entity.Id,
                            ReadingDate = dto.InstallationDate,
                            Value = dto.InitialReading,
                            EnteredAt = DateTime.Now,
                            EnteredByUserId = GetCurrentUserId(),
                            ReadingStatusId = 2,
                            TariffZone = 1,
                            Comment = "Начальное показание при установке счетчика"
                        };

                        _context.MeterReading.Add(initialReading);
                        _context.SaveChanges();

                        System.Diagnostics.Debug.WriteLine($"Создано начальное показание: {dto.InitialReading} от {dto.InstallationDate}");
                    }

                    transaction.Commit();

                    AuditLogger.Log("INSERT", "Meter", entity.Id, null,
                        new { dto.SerialNumber, dto.MeterTypeId, dto.ConsumptionObjectId, dto.InitialReading });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении счетчика: {ex.Message}");
                    throw;
                }
            }
        }

        public void Update(MeterDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task UpdateAsync(MeterDto dto)
        {
            try
            {
                var entity = await _context.Meter.FindAsync(dto.Id);
                if (entity != null)
                {
                    var oldValues = new { entity.SerialNumber, entity.MeterStatusId, entity.NextVerificationDate };
                    var newValues = new { dto.SerialNumber, dto.StatusId, dto.NextVerificationDate };

                    entity.SerialNumber = dto.SerialNumber;
                    entity.MeterTypeId = dto.MeterTypeId;
                    entity.ConsumptionObjectId = dto.ConsumptionObjectId;
                    entity.InstallationDate = dto.InstallationDate;
                    entity.InitialReading = dto.InitialReading;
                    entity.VerificationDate = dto.LastVerificationDate;
                    entity.NextVerificationDate = dto.NextVerificationDate;
                    entity.MeterStatusId = dto.StatusId;
                    entity.ServiceLifeYears = dto.ServiceLifeYears;

                    await _context.SaveChangesAsync();

                    AuditLogger.Log("UPDATE", "Meter", entity.Id, oldValues, newValues);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MeterRepository.DeleteAsync: удаление счетчика ID={id}");

                var entity = await _context.Meter.FindAsync(id);
                if (entity != null)
                {
                    var oldValues = new { entity.SerialNumber };

                    _context.Meter.Remove(entity);
                    await _context.SaveChangesAsync();

                    AuditLogger.Log("DELETE", "Meter", id, oldValues, null);

                    System.Diagnostics.Debug.WriteLine("MeterRepository.DeleteAsync: удаление выполнено успешно");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в DeleteAsync: {ex.Message}");
                throw;
            }
        }

        public List<MeterForReadingDto> GetMetersForReading(int objectId)
        {
            return GetMetersForReadingAsync(objectId).Result;
        }

        public async Task<List<MeterForReadingDto>> GetMetersForReadingAsync(int objectId)
        {
            var meters = await Query<Meter>()
                .Where(m => m.ConsumptionObjectId == objectId)
                .Select(m => new MeterForReadingDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeName = m.MeterType.Name,
                    LastReading = _context.MeterReading
                        .Where(r => r.MeterId == m.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .Select(r => (decimal?)r.Value)
                        .FirstOrDefault(),
                    LastReadingDate = _context.MeterReading
                        .Where(r => r.MeterId == m.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .Select(r => (DateTime?)r.ReadingDate)
                        .FirstOrDefault(),
                    StatusName = m.MeterStatus.Name,
                    InitialReading = m.InitialReading,
                    InstallationDate = m.InstallationDate
                })
                .ToListAsync();

            return meters;
        }

        private int GetCurrentUserId()
        {
            return 1;
        }
    }
}