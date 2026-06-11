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
    public class ObjectTypeRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        private const string CACHE_KEY_ALL = "ObjectTypes_All";
        private const string CACHE_KEY_BY_ID = "ObjectType_{0}";
        private const int CACHE_MINUTES = 60;

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                // ✅ Сначала получаем данные из БД
                var data = await Query<ObjectType>()
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.NormConsumption
                    })
                    .OrderBy(o => o.Name)
                    .ToListAsync(cancellationToken);

                // ✅ Форматируем Description уже в памяти (после получения из БД)
                return data.Select(o => new DirectoryDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    Description = o.NormConsumption.HasValue
                        ? "Норма: " + o.NormConsumption.Value.ToString("F2") + " кВт·ч/мес"
                        : null,
                    IsActive = true
                }).ToList();
            }, CACHE_MINUTES);
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var entity = await Query<ObjectType>()
                    .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

                if (entity == null) return null;

                return new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.NormConsumption.HasValue
                        ? "Норма: " + entity.NormConsumption.Value.ToString("F2") + " кВт·ч/мес"
                        : null,
                    IsActive = true
                };
            }, CACHE_MINUTES);
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название типа объекта не может быть пустым");

            var entity = new ObjectType { Name = dto.Name.Trim() };
            _context.ObjectType.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();

            AuditLogger.Log("INSERT", "ObjectType", entity.Id, null, new { dto.Name });

            return entity.Id;
        }

        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название типа объекта не может быть пустым");

            var entity = await _context.ObjectType.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.Name };
            var newValues = new { dto.Name };

            entity.Name = dto.Name.Trim();
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "ObjectType", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.ObjectType.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            // Проверяем, есть ли связанные объекты
            bool hasObjects = await Query<ConsumptionObject>()
                .AnyAsync(o => o.ObjectTypeId == id, cancellationToken);

            if (hasObjects)
            {
                throw new InvalidOperationException("Нельзя удалить тип объекта, который используется");
            }

            var oldValues = new { entity.Name };

            _context.ObjectType.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "ObjectType", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL);
        }
    }
}