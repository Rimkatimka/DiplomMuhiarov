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
        // Константы для кэширования
        private const string CACHE_KEY_ALL_METERS = "AllMeters";
        private const string CACHE_KEY_METER_BY_ID = "Meter_{0}";
        private const string CACHE_KEY_METERS_BY_OBJECT = "MetersByObject_{0}";
        private const int CACHE_MINUTES = 30;
        private const int DEFAULT_CURRENT_USER_ID = 1;

        // Синхронные методы (для совместимости)
        public List<MeterDto> GetByObjectId(int objectId)
        {
            return GetByObjectIdAsync(objectId).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetByObjectId с кэшированием
        public async Task<List<MeterDto>> GetByObjectIdAsync(int objectId, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_METERS_BY_OBJECT, objectId);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
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
            }, CACHE_MINUTES);
        }

        public List<MeterDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetAll с кэшированием
        public async Task<List<MeterDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL_METERS, async () =>
            {
                try
                {
                    return await Query<Meter>()
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
                    System.Diagnostics.Debug.WriteLine($"Ошибка в GetAllAsync: {ex.Message}");
                    return new List<MeterDto>();
                }
            }, CACHE_MINUTES);
        }

        public MeterDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetById с кэшированием
        public async Task<MeterDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_METER_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
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
            }, CACHE_MINUTES);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с async
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
                            ReadingStatusId = 2, // Подтверждено
                            TariffZone = 1,
                            Comment = "Начальное показание при установке счетчика"
                        };

                        _context.MeterReading.Add(initialReading);
                        await _context.SaveChangesAsync(cancellationToken);

                        System.Diagnostics.Debug.WriteLine($"Создано начальное показание: {dto.InitialReading} от {dto.InstallationDate}");
                    }

                    transaction.Commit();

                    // Инвалидируем кэш
                    InvalidateCache(dto.ConsumptionObjectId);

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

        // Синхронный Add (для совместимости)
        public void Add(MeterDto dto)
        {
            AddAsync(dto).Wait();
        }

        public void Update(MeterDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Update с async и инвалидацией кэша
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

                // Инвалидируем кэш
                InvalidateCache(dto.ConsumptionObjectId);
                CacheService.Remove(string.Format(CACHE_KEY_METER_BY_ID, dto.Id));

                AuditLogger.Log("UPDATE", "Meter", entity.Id, oldValues, newValues);

                return true;
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

        // ✅ ОПТИМИЗИРОВАННЫЙ Delete с async
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MeterRepository.DeleteAsync: удаление счетчика ID={id}");

                var entity = await _context.Meter.FindAsync(cancellationToken, id);
                if (entity == null) return false;

                // Проверяем, есть ли связанные показания
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

                // Инвалидируем кэш
                InvalidateCache(objectId);
                CacheService.Remove(string.Format(CACHE_KEY_METER_BY_ID, id));

                AuditLogger.Log("DELETE", "Meter", id, oldValues, null);

                System.Diagnostics.Debug.WriteLine("MeterRepository.DeleteAsync: удаление выполнено успешно");
                return true;
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

        // ✅ ОПТИМИЗИРОВАННЫЙ GetMetersForReading с кэшированием
        public async Task<List<MeterForReadingDto>> GetMetersForReadingAsync(int objectId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"{CACHE_KEY_METERS_BY_OBJECT}_{objectId}_ForReading";

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
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
            }, CACHE_MINUTES);
        }

        // ✅ НОВЫЙ МЕТОД: получение счетчиков с пагинацией
        public async Task<PaginatedResult<MeterDto>> GetPaginatedAsync(
            int page,
            int pageSize,
            int? objectId = null,
            int? statusId = null,
            string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query<Meter>().AsQueryable();

            if (objectId.HasValue && objectId.Value > 0)
            {
                query = query.Where(m => m.ConsumptionObjectId == objectId.Value);
            }

            if (statusId.HasValue && statusId.Value > 0)
            {
                query = query.Where(m => m.MeterStatusId == statusId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m => m.SerialNumber.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(m => m.SerialNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .ToListAsync(cancellationToken);

            return new PaginatedResult<MeterDto>(items, totalCount, page, pageSize);
        }

        // Приватный метод инвалидации кэша
        private void InvalidateCache(int objectId)
        {
            CacheService.Remove(CACHE_KEY_ALL_METERS);
            CacheService.Remove(string.Format(CACHE_KEY_METERS_BY_OBJECT, objectId));
            CacheService.Remove($"{CACHE_KEY_METERS_BY_OBJECT}_{objectId}_ForReading");
        }

        private int GetCurrentUserId()
        {
            // TODO: получить реального пользователя из контекста
            return DEFAULT_CURRENT_USER_ID;
        }
    }
}