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
        public async Task<List<DirectoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var data = await Query<VerificationInterval>()
                .Include(vi => vi.MeterType)
                .Select(v => new { v.Id, v.MeterType.Name, v.Years })
                .OrderBy(v => v.Name)
                .ToListAsync(cancellationToken);

            return data.Select(v => new DirectoryDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = $"Интервал: {v.Years} лет",
                IsActive = true
            }).ToList();
        }

        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await Query<VerificationInterval>()
                .Include(vi => vi.MeterType)
                .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

            return entity == null ? null : new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.MeterType.Name,
                Description = $"Интервал: {entity.Years} лет",
                IsActive = true
            };
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<int> AddAsync(DirectoryDto dto, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Название типа счетчика не может быть пустым");

            var meterType = await Query<MeterType>()
                .FirstOrDefaultAsync(mt => mt.Name == dto.Name, cancellationToken);

            if (meterType == null)
            {
                throw new InvalidOperationException($"Тип счетчика '{dto.Name}' не найден");
            }

            int years = 16;
            if (!string.IsNullOrEmpty(dto.Description))
            {
                var match = Regex.Match(dto.Description, @"(\d+)");
                if (match.Success)
                {
                    years = int.Parse(match.Groups[1].Value);
                }
            }

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

            return entity == null ? null : new DirectoryDto
            {
                Id = entity.Id,
                Name = entity.MeterType.Name,
                Description = $"Интервал: {entity.Years} лет",
                IsActive = true
            };
        }
    }
}