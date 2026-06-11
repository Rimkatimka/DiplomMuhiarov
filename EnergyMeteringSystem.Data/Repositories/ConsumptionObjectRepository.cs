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
        // Константы для кэширования
        private const string CACHE_KEY_ALL_OBJECTS = "AllConsumptionObjects";
        private const string CACHE_KEY_OBJECT_BY_ID = "ConsumptionObject_{0}";
        private const int CACHE_MINUTES = 30;

        // Синхронный (для совместимости)
        public List<ConsumptionObjectDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ АСИНХРОННЫЙ - ОДИН ЗАПРОС!
        public async Task<List<ConsumptionObjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            System.Diagnostics.Debug.WriteLine("GetAllAsync() — оптимизированный запрос");

            try
            {
                // Убираем кэш и делаем прямой запрос
                var result = await _context.ConsumptionObject
                    .Include(o => o.Street)
                    .Include(o => o.Street.City)
                    .Include(o => o.Street.City.Region)
                    .Include(o => o.ObjectType)
                    .Select(o => new ConsumptionObjectDto
                    {
                        Id = o.Id,
                        StreetId = o.StreetId,
                        HouseNumber = o.HouseNumber,
                        ApartmentNumber = o.ApartmentNumber,
                        ObjectTypeId = o.ObjectTypeId,
                        TotalArea = o.TotalArea,
                        ResidentCount = o.ResidentCount,
                        Street = o.Street.Name,
                        City = o.Street.City.Name,
                        CityId = o.Street.City.Id,
                        Region = o.Street.City.Region.Name,
                        RegionId = o.Street.City.Region.Id,
                        ObjectTypeName = o.ObjectType.Name
                    })
                    .OrderBy(o => o.City)
                    .ThenBy(o => o.Street)
                    .ThenBy(o => o.HouseNumber)
                    .ToListAsync(cancellationToken);

                System.Diagnostics.Debug.WriteLine($"GetAllAsync() вернул {result.Count} объектов");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync() ERROR: {ex.Message}");
                return new List<ConsumptionObjectDto>();
            }
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetById
        public async Task<ConsumptionObjectDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_OBJECT_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
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
            }, CACHE_MINUTES);
        }

        // Синхронный GetById (для совместимости)
        public ConsumptionObjectDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        // ✅ НОВЫЙ МЕТОД: получение объектов по региону
        public async Task<List<ConsumptionObjectDto>> GetByRegionIdAsync(int regionId, CancellationToken cancellationToken = default)
        {
            return await Query<ConsumptionObject>()
                .Where(o => o.Street.City.RegionId == regionId)
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    Street = o.Street.Name,
                    StreetId = o.StreetId,
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
                .ToListAsync(cancellationToken);
        }

        // ✅ НОВЫЙ МЕТОД: получение объектов с пагинацией
        public async Task<PaginatedResult<ConsumptionObjectDto>> GetPaginatedAsync(
            int page,
            int pageSize,
            int? regionId = null,
            int? cityId = null,
            string searchTerm = null,
            CancellationToken cancellationToken = default)
        {
            var query = Query<ConsumptionObject>()
                .Include(o => o.Street)
                .Include(o => o.Street.City)
                .Include(o => o.Street.City.Region)
                .Include(o => o.ObjectType)
                .AsQueryable();

            // Фильтры
            if (regionId.HasValue && regionId.Value > 0)
            {
                query = query.Where(o => o.Street.City.RegionId == regionId.Value);
            }

            if (cityId.HasValue && cityId.Value > 0)
            {
                query = query.Where(o => o.Street.CityId == cityId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(o =>
                    o.Street.Name.Contains(searchTerm) ||
                    o.HouseNumber.Contains(searchTerm) ||
                    o.Street.City.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(o => o.Street.City.Name)
                .ThenBy(o => o.Street.Name)
                .ThenBy(o => o.HouseNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    Street = o.Street.Name,
                    StreetId = o.StreetId,
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
                .ToListAsync(cancellationToken);

            return new PaginatedResult<ConsumptionObjectDto>(items, totalCount, page, pageSize);
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Add с async
        public async Task<int> AddAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
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
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();

            AuditLogger.Log("INSERT", "ConsumptionObject", entity.Id, null,
                new { dto.HouseNumber, dto.ApartmentNumber, dto.ObjectTypeId });

            return entity.Id;
        }

        // Синхронный Add (для совместимости)
        public void Add(ConsumptionObjectDto dto)
        {
            AddAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Delete с async
        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем наличие связанных данных
            bool hasMeters = await Query<Meter>().AnyAsync(m => m.ConsumptionObjectId == id, cancellationToken);
            if (hasMeters)
            {
                throw new System.InvalidOperationException("Нельзя удалить объект, у которого есть счетчики");
            }

            var oldValues = new { entity.HouseNumber, entity.ApartmentNumber };

            _context.ConsumptionObject.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();

            AuditLogger.Log("DELETE", "ConsumptionObject", id, oldValues, null);

            return true;
        }

        // Синхронный Delete (для совместимости)
        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Update с async
        public async Task<bool> UpdateAsync(ConsumptionObjectDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ConsumptionObject.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new
            {
                entity.HouseNumber,
                entity.ApartmentNumber,
                entity.TotalArea,
                entity.ResidentCount,
                entity.StreetId,
                entity.ObjectTypeId
            };

            var newValues = new
            {
                dto.HouseNumber,
                dto.ApartmentNumber,
                dto.TotalArea,
                dto.ResidentCount,
                dto.StreetId,
                dto.ObjectTypeId
            };

            entity.StreetId = dto.StreetId;
            entity.HouseNumber = dto.HouseNumber?.Trim();
            entity.ApartmentNumber = dto.ApartmentNumber?.Trim();
            entity.ObjectTypeId = dto.ObjectTypeId;
            entity.TotalArea = dto.TotalArea;
            entity.ResidentCount = dto.ResidentCount;

            await _context.SaveChangesAsync(cancellationToken);

            // Инвалидируем кэш
            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_OBJECT_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "ConsumptionObject", entity.Id, oldValues, newValues);

            return true;
        }

        // Синхронный Update (для совместимости)
        public void Update(ConsumptionObjectDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        // ✅ НОВЫЙ МЕТОД: поиск объектов по адресу
        public async Task<List<ConsumptionObjectDto>> SearchByAddressAsync(
            string searchTerm,
            int maxResults = 50,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<ConsumptionObjectDto>();

            return await Query<ConsumptionObject>()
                .Where(o =>
                    o.Street.Name.Contains(searchTerm) ||
                    o.HouseNumber.Contains(searchTerm) ||
                    o.Street.City.Name.Contains(searchTerm) ||
                    o.Street.City.Region.Name.Contains(searchTerm))
                .Take(maxResults)
                .Select(o => new ConsumptionObjectDto
                {
                    Id = o.Id,
                    Street = o.Street.Name,
                    StreetId = o.StreetId,
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
                .ToListAsync(cancellationToken);
        }

        // ✅ НОВЫЙ МЕТОД: получение количества объектов
        public async Task<int> GetCountAsync(int? regionId = null, CancellationToken cancellationToken = default)
        {
            var query = Query<ConsumptionObject>().AsQueryable();

            if (regionId.HasValue && regionId.Value > 0)
            {
                query = query.Where(o => o.Street.City.RegionId == regionId.Value);
            }

            return await query.CountAsync(cancellationToken);
        }

        // Приватный метод инвалидации кэша
        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL_OBJECTS);
            // Можно также очистить кэш по паттерну
            // Но лучше использовать теги кэша, если нужны
        }
    }
}