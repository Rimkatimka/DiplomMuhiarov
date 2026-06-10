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
    public class VerificationIntervalRepository : BaseRepository, IDirectoryRepository<DirectoryDto>
    {
        public List<DirectoryDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<DirectoryDto>> GetAllAsync()
        {
            var data = await Query<VerificationInterval>()
                .Include(vi => vi.MeterType)
                .Select(v => new { v.Id, v.MeterType.Name, v.Years })
                .ToListAsync();

            return data.Select(v => new DirectoryDto
            {
                Id = v.Id,
                Name = v.Name,
                Description = "Интервал: " + v.Years + " лет",
                IsActive = true
            }).ToList();
        }

        public DirectoryDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<DirectoryDto> GetByIdAsync(int id)
        {
            var entity = await Query<VerificationInterval>()
                .Include(vi => vi.MeterType)
                .FirstOrDefaultAsync(v => v.Id == id);

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

        public void Add(DirectoryDto dto)
        {
            // Нужно получить MeterTypeId по имени типа счетчика
            var meterType = _context.MeterType.FirstOrDefault(mt => mt.Name == dto.Name);
            if (meterType == null)
            {
                throw new InvalidOperationException($"Тип счетчика '{dto.Name}' не найден");
            }

            // Извлекаем количество лет из описания (например, "Интервал: 16 лет")
            int years = 16; // значение по умолчанию
            if (!string.IsNullOrEmpty(dto.Description))
            {
                var match = System.Text.RegularExpressions.Regex.Match(dto.Description, @"(\d+)");
                if (match.Success)
                {
                    years = int.Parse(match.Groups[1].Value);
                }
            }

            var entity = new VerificationInterval
            {
                MeterTypeId = meterType.Id,
                Years = years
            };
            _context.VerificationInterval.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "VerificationInterval", entity.Id, null,
                new { MeterTypeId = meterType.Id, Years = years });
        }

        public void Update(DirectoryDto dto)
        {
            var entity = _context.VerificationInterval.Find(dto.Id);
            if (entity != null)
            {
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.VerificationInterval.Find(id);
            if (entity != null)
            {
                _context.VerificationInterval.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}