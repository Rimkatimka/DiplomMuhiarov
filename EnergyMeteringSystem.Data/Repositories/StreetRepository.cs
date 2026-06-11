using EnergyMeteringSystem.Core.Helpers;
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
    public class StreetRepository : BaseRepository
    {
        private const string CACHE_KEY_ALL = "Streets_All";
        private const string CACHE_KEY_BY_ID = "Street_{0}";
        private const string CACHE_KEY_BY_CITY = "Streets_ByCity_{0}";
        private const int CACHE_MINUTES = 60;

        public List<StreetDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<StreetDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                return await Query<Street>()
                    .Select(s => new StreetDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        CityId = s.CityId,
                        CityName = s.City.Name,
                        PostalCode = s.PostalCode
                    })
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES);
        }

        public List<StreetDto> GetByCityId(int cityId)
        {
            return GetByCityIdAsync(cityId).Result;
        }

        public async Task<List<StreetDto>> GetByCityIdAsync(int cityId, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_CITY, cityId);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                return await Query<Street>()
                    .Where(s => s.CityId == cityId)
                    .Select(s => new StreetDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        CityId = s.CityId,
                        CityName = s.City.Name,
                        PostalCode = s.PostalCode
                    })
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES);
        }

        public StreetDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<StreetDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"GetByIdAsync: ищем улицу с ID={id}");

                    var s = await Query<Street>()
                        .Include(x => x.City)
                        .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                    if (s == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"GetByIdAsync: улица с ID={id} не найдена");
                        return null;
                    }

                    System.Diagnostics.Debug.WriteLine($"GetByIdAsync: найдена улица '{s.Name}', CityId={s.CityId}");

                    string cityName = string.Empty;
                    if (s.City != null)
                    {
                        cityName = s.City.Name;
                        System.Diagnostics.Debug.WriteLine($"GetByIdAsync: город '{cityName}'");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"GetByIdAsync: City == null, пробуем загрузить отдельно");
                        var city = await Query<City>().FirstOrDefaultAsync(c => c.Id == s.CityId, cancellationToken);
                        cityName = city?.Name ?? "Неизвестно";
                    }

                    return new StreetDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        CityId = s.CityId,
                        CityName = cityName,
                        PostalCode = s.PostalCode
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetByIdAsync ОШИБКА: {ex.Message}");
                    throw;
                }
            }, CACHE_MINUTES);
        }

        public async Task<int> AddAsync(StreetDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название улицы не может быть пустым");

            var entity = new Street
            {
                Name = dto.Name.Trim(),
                CityId = dto.CityId,
                PostalCode = dto.PostalCode?.Trim()
            };
            _context.Street.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache(dto.CityId);

            AuditLogger.Log("INSERT", "Street", entity.Id, null, new { dto.Name, dto.CityId });

            return entity.Id;
        }

        public void Add(StreetDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Street.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные объекты
            bool hasObjects = await Query<ConsumptionObject>()
                .AnyAsync(o => o.StreetId == id, cancellationToken);

            if (hasObjects)
            {
                throw new InvalidOperationException("Нельзя удалить улицу, на которой есть объекты");
            }

            var oldValues = new { entity.Name };
            int cityId = entity.CityId;

            _context.Street.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache(cityId);
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "Street", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<List<StreetDto>> SearchAsync(string searchTerm, int cityId = 0, int maxResults = 20, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return cityId > 0 ? await GetByCityIdAsync(cityId, cancellationToken) : await GetAllAsync(cancellationToken);

            var query = Query<Street>().Where(s => s.Name.Contains(searchTerm));

            if (cityId > 0)
            {
                query = query.Where(s => s.CityId == cityId);
            }

            return await query
                .Take(maxResults)
                .Select(s => new StreetDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    CityId = s.CityId,
                    CityName = s.City.Name,
                    PostalCode = s.PostalCode
                })
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        private void InvalidateCache(int cityId)
        {
            CacheService.Remove(CACHE_KEY_ALL);
            CacheService.Remove(string.Format(CACHE_KEY_BY_CITY, cityId));
        }
    }
}