using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class RegionRepository : BaseRepository
    {
        public List<RegionDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<RegionDto>> GetAllAsync()
        {
            return await Query<Region>()
                .Select(r => new RegionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code
                })
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public RegionDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<RegionDto> GetByIdAsync(int id)
        {
            var region = await Query<Region>()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (region == null) return null;

            return new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                Code = region.Code
            };
        }

        public RegionDto GetByName(string name)
        {
            return GetByNameAsync(name).Result;
        }

        public async Task<RegionDto> GetByNameAsync(string name)
        {
            var region = await Query<Region>()
                .FirstOrDefaultAsync(r => r.Name == name);

            if (region == null) return null;

            return new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                Code = region.Code
            };
        }

        public bool Exists(string name)
        {
            return ExistsAsync(name).Result;
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await Query<Region>().AnyAsync(r => r.Name == name);
        }

        public void Add(RegionDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task AddAsync(RegionDto dto)
        {
            if (await ExistsAsync(dto.Name))
            {
                throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
            }

            var entity = new Region
            {
                Name = dto.Name,
                Code = dto.Code
            };
            _context.Region.Add(entity);
            await _context.SaveChangesAsync();

            AuditLogger.Log("INSERT", "Region", entity.Id, null, new { dto.Name, dto.Code });
        }

        public void Update(RegionDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task UpdateAsync(RegionDto dto)
        {
            var entity = await _context.Region.FindAsync(dto.Id);
            if (entity != null)
            {
                if (entity.Name != dto.Name && await ExistsAsync(dto.Name))
                {
                    throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
                }

                var oldValues = new { entity.Name, entity.Code };
                var newValues = new { dto.Name, dto.Code };

                entity.Name = dto.Name;
                entity.Code = dto.Code;
                await _context.SaveChangesAsync();

                AuditLogger.Log("UPDATE", "Region", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Region.FindAsync(id);
            if (entity != null)
            {
                bool hasCities = await Query<City>().AnyAsync(c => c.RegionId == id);
                if (hasCities)
                {
                    throw new InvalidOperationException("Нельзя удалить регион, в котором есть города");
                }

                var oldValues = new { entity.Name };

                _context.Region.Remove(entity);
                await _context.SaveChangesAsync();

                AuditLogger.Log("DELETE", "Region", id, oldValues, null);
            }
        }
    }
}