using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterRepository : BaseRepository, IMeterRepository
    {
        private const int DEFAULT_CURRENT_USER_ID = 1;

        public async Task<List<MeterDto>> GetByObjectIdAsync(int objectId, CancellationToken cancellationToken = default)
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
                    .OrderBy(m => m.SerialNumber)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetByObjectIdAsync: {ex.Message}");
                return new List<MeterDto>();
            }
        }

        public List<MeterDto> GetByObjectId(int objectId)
        {
            return GetByObjectIdAsync(objectId).Result;
        }

        public async Task<List<MeterDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Meter
                    .Include(m => m.MeterType)
                    .Include(m => m.MeterStatus)
                    .Select(m => new MeterDto
                    {
                        Id = m.Id,
                        SerialNumber = m.SerialNumber,
                        MeterTypeId = m.MeterTypeId,
                        MeterTypeName = m.MeterType.Name,
                        StatusId = m.MeterStatusId,
                        StatusName = m.MeterStatus.Name,
                        InstallationDate = m.InstallationDate,
                        LastVerificationDate = m.VerificationDate,
                        NextVerificationDate = m.NextVerificationDate,
                        InitialReading = m.InitialReading,
                        ServiceLifeYears = m.ServiceLifeYears,
                        ConsumptionObjectId = m.ConsumptionObjectId
                    })
                    .OrderBy(m => m.SerialNumber)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MeterRepository.GetAllAsync ERROR: {ex.Message}");
                return new List<MeterDto>();
            }
        }

        public List<MeterDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<MeterDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var meter = await Query<Meter>()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

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

        public MeterDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(MeterDto dto, CancellationToken cancellationToken = default)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var entity = new Meter
                    {
                        SerialNumber = dto.SerialNumber?.Trim(),
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
                    await _context.SaveChangesAsync(cancellationToken);

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
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    transaction.Commit();

                    AuditLogger.Log("INSERT", "Meter", entity.Id, null,
                        new { dto.SerialNumber, dto.MeterTypeId, dto.ConsumptionObjectId, dto.InitialReading });

                    return entity.Id;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении счетчика: {ex.Message}");
                    throw;
                }
            }
        }

        public void Add(MeterDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(MeterDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.Meter.FindAsync(cancellationToken, dto.Id);
                if (entity == null) return false;

                var oldValues = new { entity.SerialNumber, entity.MeterStatusId, entity.NextVerificationDate };
                var newValues = new { dto.SerialNumber, dto.StatusId, dto.NextVerificationDate };

                entity.SerialNumber = dto.SerialNumber?.Trim();
                entity.MeterTypeId = dto.MeterTypeId;
                entity.ConsumptionObjectId = dto.ConsumptionObjectId;
                entity.InstallationDate = dto.InstallationDate;
                entity.InitialReading = dto.InitialReading;
                entity.VerificationDate = dto.LastVerificationDate;
                entity.NextVerificationDate = dto.NextVerificationDate;
                entity.MeterStatusId = dto.StatusId;
                entity.ServiceLifeYears = dto.ServiceLifeYears;

                await _context.SaveChangesAsync(cancellationToken);

                AuditLogger.Log("UPDATE", "Meter", entity.Id, oldValues, newValues);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в UpdateAsync: {ex.Message}");
                throw;
            }
        }

        public void Update(MeterDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.Meter.FindAsync(cancellationToken, id);
                if (entity == null) return false;

                bool hasReadings = await Query<MeterReading>()
                    .AnyAsync(r => r.MeterId == id, cancellationToken);

                if (hasReadings)
                {
                    throw new InvalidOperationException("Нельзя удалить счетчик, у которого есть показания");
                }

                var oldValues = new { entity.SerialNumber };
                int objectId = entity.ConsumptionObjectId;

                _context.Meter.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);

                AuditLogger.Log("DELETE", "Meter", id, oldValues, null);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в DeleteAsync: {ex.Message}");
                throw;
            }
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<List<MeterForReadingDto>> GetMetersForReadingAsync(int objectId, CancellationToken cancellationToken = default)
        {
            var meters = await Query<Meter>()
                .Where(m => m.ConsumptionObjectId == objectId)
                .Select(m => new MeterForReadingDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeName = m.MeterType.Name,
                    LastReading = m.MeterReading
                        .Where(r => r.MeterId == m.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .Select(r => (decimal?)r.Value)
                        .FirstOrDefault(),
                    LastReadingDate = m.MeterReading
                        .Where(r => r.MeterId == m.Id)
                        .OrderByDescending(r => r.ReadingDate)
                        .Select(r => (DateTime?)r.ReadingDate)
                        .FirstOrDefault(),
                    StatusName = m.MeterStatus.Name,
                    InitialReading = m.InitialReading,
                    InstallationDate = m.InstallationDate
                })
                .OrderBy(m => m.SerialNumber)
                .ToListAsync(cancellationToken);

            return meters;
        }

        public List<MeterForReadingDto> GetMetersForReading(int objectId)
        {
            return GetMetersForReadingAsync(objectId).Result;
        }

        private int GetCurrentUserId()
        {
            return DEFAULT_CURRENT_USER_ID;
        }
    }
}