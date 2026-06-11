using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class VerificationIntervalRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        private const string CACHE_KEY_ALL = "VerificationIntervals_All";
        private const string CACHE_KEY_BY_ID = "VerificationInterval_{0}";
        private const int CACHE_MINUTES = 60;

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL, async () =>
            {
                var data = await Query<VerificationInterval>()
                    .Include(vi => vi.MeterType)
                    .Select(v => new DirectoryDto
                    {
                        Id = v.Id,
                        Name = v.MeterType.Name,
                        Description = $"Интервал: {v.Years} лет",
                        IsActive = true
                    })
                    .OrderBy(v => v.Name)
                    .ToListAsync(cancellationToken);

                return data;
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
                var entity = await Query<VerificationInterval>()
                    .Include(vi => vi.MeterType)
                    .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

                return entity == null
                    ? null
                    : new DirectoryDto
                    {
                        Id = entity.Id,
                        Name = entity.MeterType.Name,
                        Description = $"Интервал: {entity.Years} лет",
                        IsActive = true
                    };
            }, CACHE_MINUTES);
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название типа счетчика не может быть пустым");

            // Находим MeterType по имени
            var meterType = await Query<MeterType>()
                .FirstOrDefaultAsync(mt => mt.Name == dto.Name, cancellationToken);

            if (meterType == null)
            {
                throw new InvalidOperationException($"Тип счетчика '{dto.Name}' не найден");
            }

            // Извлекаем количество лет из описания
            int years = 16; // значение по умолчанию
            if (!string.IsNullOrEmpty(dto.Description))
            {
                var match = Regex.Match(dto.Description, @"(\d+)");
                if (match.Success)
                {
                    years = int.Parse(match.Groups[1].Value);
                }
            }

            // Проверяем, не существует ли уже интервал для этого типа
            bool exists = await Query<VerificationInterval>()
                .AnyAsync(v => v.MeterTypeId == meterType.Id, cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException($"Интервал поверки для типа '{dto.Name}' уже существует");
            }

            var entity = new VerificationInterval
            {
                MeterTypeId = meterType.Id,
                Years = years
            };
            _context.VerificationInterval.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();

            AuditLogger.Log("INSERT", "VerificationInterval", entity.Id, null,
                new { MeterTypeId = meterType.Id, Years = years });

            return entity.Id;
        }

        public void Add(DirectoryDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.VerificationInterval.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            // Извлекаем количество лет из описания
            int years = entity.Years;
            if (!string.IsNullOrEmpty(dto.Description))
            {
                var match = Regex.Match(dto.Description, @"(\d+)");
                if (match.Success)
                {
                    years = int.Parse(match.Groups[1].Value);
                }
            }

            var oldValues = new { entity.Years };
            var newValues = new { Years = years };

            entity.Years = years;
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, dto.Id));

            AuditLogger.Log("UPDATE", "VerificationInterval", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(DirectoryDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.VerificationInterval.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            var oldValues = new { entity.Years };

            _context.VerificationInterval.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();
            CacheService.Remove(string.Format(CACHE_KEY_BY_ID, id));

            AuditLogger.Log("DELETE", "VerificationInterval", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<DirectoryDto> GetByMeterTypeIdAsync(int meterTypeId, CancellationToken cancellationToken = default)
        {
            var entity = await Query<VerificationInterval>()
                .Include(vi => vi.MeterType)
                .FirstOrDefaultAsync(v => v.MeterTypeId == meterTypeId, cancellationToken);

            return entity == null
                ? null
                : new DirectoryDto
                {
                    Id = entity.Id,
                    Name = entity.MeterType.Name,
                    Description = $"Интервал: {entity.Years} лет",
                    IsActive = true
                };
        }

        private void InvalidateCache()
        {
            CacheService.Remove(CACHE_KEY_ALL);
        }
    }
}