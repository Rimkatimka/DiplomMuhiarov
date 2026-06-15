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
    public class ConsumptionObjectRepository : BaseRepository, IConsumptionObjectRepository
    {
        public async Task<List<ConsumptionObjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.ConsumptionObject
                    .Include(o => o.Street)
                    .Include(o => o.Street.City)
                    .Include(o => o.Street.City.Region)
                    .Include(o => o.ObjectType)
                    .Select(o => new ConsumptionObjectDto
                    {
                        Id = o.Id,
                        StreetId = o.StreetId,
                        Street = o.Street.Name,
                        City = o.Street.City.Name,
                        CityId = o.Street.City.Id,
                        Region = o.Street.City.Region.Name,
                        RegionId = o.Street.City.Region.Id,
                        HouseNumber = o.HouseNumber,
                        ApartmentNumber = o.ApartmentNumber,
                        ObjectTypeId = o.ObjectTypeId,
                        ObjectTypeName = o.ObjectType.Name,
                        TotalArea = o.TotalArea,
                        ResidentCount = o.ResidentCount
                    })
                    .OrderBy(o => o.City)
                    .ThenBy(o => o.Street)
                    .ThenBy(o => o.HouseNumber)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync() ERROR: {ex.Message}");
                return new List<ConsumptionObjectDto>();
            }
        }

        public List<ConsumptionObjectDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<ConsumptionObjectDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await Query<ConsumptionObject>()
                .Where(o => o.Id == id)
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    StreetId = o.StreetId,
                    Street = o.Street.Name,
                    City = o.Street.City.Name,
                    CityId = o.Street.City.Id,
                    Region = o.Street.City.Region.Name,
                    RegionId = o.Street.City.Region.Id,
                    HouseNumber = o.HouseNumber,
                    ApartmentNumber = o.ApartmentNumber,
                    ObjectTypeId = o.ObjectTypeId,
                    ObjectTypeName = o.ObjectType.Name,
                    TotalArea = o.TotalArea,
                    ResidentCount = o.ResidentCount
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public ConsumptionObjectDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] AddAsync НАЧАЛО");

            var entity = new ConsumptionObject
            {
                StreetId = dto.StreetId,
                HouseNumber = dto.HouseNumber?.Trim(),
                ApartmentNumber = dto.ApartmentNumber?.Trim(),
                ObjectTypeId = dto.ObjectTypeId,
                TotalArea = dto.TotalArea,
                ResidentCount = dto.ResidentCount
            };

            _context.ConsumptionObject.Add(entity);

            _context.Configuration.AutoDetectChangesEnabled = true;

            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token))
                {
                    await _context.SaveChangesAsync(linkedCts.Token);
                }
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Сохранено! Id={entity.Id}");
                return entity.Id;
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Таймаут операции!");
                ForceKillConnection(); // Закрываем соединение
                throw new TimeoutException("Сохранение заняло слишком много времени");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Ошибка: {ex.Message}");
                ForceKillConnection(); // Закрываем соединение при ошибке
                throw;
            }
            finally
            {
                _context.Configuration.AutoDetectChangesEnabled = false;
            }
        }

        public void Add(ConsumptionObjectDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

            if (entity == null) return false;

            // Проверяем наличие счетчиков (быстро)
            bool hasMeters = await _context.Meter
                .AnyAsync(m => m.ConsumptionObjectId == id, cancellationToken);

            if (hasMeters)
            {
                throw new InvalidOperationException("Нельзя удалить объект, у которого есть счетчики");
            }

            var oldValues = new { entity.HouseNumber, entity.ApartmentNumber };

            _context.ConsumptionObject.Remove(entity);

            _context.Configuration.AutoDetectChangesEnabled = true;
            var result = await _context.SaveChangesAsync(cancellationToken);
            _context.Configuration.AutoDetectChangesEnabled = false;

            if (result > 0)
            {
                AuditLogger.Log("DELETE", "ConsumptionObject", id, oldValues, null);
            }

            return result > 0;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        

        public void Update(ConsumptionObjectDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        /// <summary>
        /// Получить объекты с фильтрацией (оптимизированный метод)
        /// </summary>
        public async Task<List<ConsumptionObjectDto>> GetFilteredAsync(int? regionId = null, int? cityId = null, int? streetId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.ConsumptionObject
                .Include(o => o.Street)
                .Include(o => o.Street.City)
                .Include(o => o.Street.City.Region)
                .Include(o => o.ObjectType)
                .AsQueryable();

            if (regionId.HasValue && regionId.Value > 0)
            {
                query = query.Where(o => o.Street.City.RegionId == regionId.Value);
            }

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(o => o.Street.CityId == cityId.Value);
            }

            if (streetId.HasValue && streetId.Value > 0)
            {
                query = query.Where(o => o.StreetId == streetId.Value);
            }

            return await query
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    StreetId = o.StreetId,
                    Street = o.Street.Name,
                    City = o.Street.City.Name,
                    CityId = o.Street.City.Id,
                    Region = o.Street.City.Region.Name,
                    RegionId = o.Street.City.Region.Id,
                    HouseNumber = o.HouseNumber,
                    ApartmentNumber = o.ApartmentNumber,
                    ObjectTypeId = o.ObjectTypeId,
                    ObjectTypeName = o.ObjectType.Name,
                    TotalArea = o.TotalArea,
                    ResidentCount = o.ResidentCount
                })
                .OrderBy(o => o.City)
                .ThenBy(o => o.Street)
                .ThenBy(o => o.HouseNumber)
                .ToListAsync(cancellationToken);
        }
        public async Task<bool> UpdateAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateAsync НАЧАЛО, Id={dto.Id}");

            var entity = await _context.ConsumptionObject
                .FirstOrDefaultAsync(o => o.Id == dto.Id, cancellationToken);

            if (entity == null)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Объект с Id={dto.Id} не найден");
                return false;
            }

            entity.StreetId = dto.StreetId;
            entity.HouseNumber = dto.HouseNumber?.Trim();
            entity.ApartmentNumber = dto.ApartmentNumber?.Trim();
            entity.ObjectTypeId = dto.ObjectTypeId;
            entity.TotalArea = dto.TotalArea;
            entity.ResidentCount = dto.ResidentCount;

            _context.Configuration.AutoDetectChangesEnabled = true;

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

                var result = await _context.SaveChangesAsync(linkedCts.Token);
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateAsync УСПЕШНО, result={result}");
                return result > 0;
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateAsync ТАЙМАУТ");
                ForceKillConnection();
                throw new TimeoutException("Сохранение заняло слишком много времени");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] UpdateAsync ОШИБКА: {ex.Message}");
                ForceKillConnection();
                throw;
            }
            finally
            {
                _context.Configuration.AutoDetectChangesEnabled = false;
            }
        }

    }
}