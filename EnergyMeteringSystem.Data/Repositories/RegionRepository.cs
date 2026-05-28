using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class RegionRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public RegionRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<RegionDto> GetAll()
        {
            return _context.Region
                .Select(r => new RegionDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code
                })
                .OrderBy(r => r.Name)
                .ToList();
        }

        public RegionDto GetById(int id)
        {
            var region = _context.Region.Find(id);
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
            var region = _context.Region.FirstOrDefault(r => r.Name == name);
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
            return _context.Region.Any(r => r.Name == name);
        }

        public void Add(RegionDto dto)
        {
            // Проверяем, существует ли уже регион с таким названием
            if (Exists(dto.Name))
            {
                throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
            }

            var entity = new Region
            {
                Name = dto.Name,
                Code = dto.Code
            };
            _context.Region.Add(entity);
            _context.SaveChanges();
        }

        public void Update(RegionDto dto)
        {
            var entity = _context.Region.Find(dto.Id);
            if (entity != null)
            {
                // Проверяем, не пытаемся ли переименовать в уже существующее название
                if (entity.Name != dto.Name && Exists(dto.Name))
                {
                    throw new InvalidOperationException($"Регион '{dto.Name}' уже существует в базе данных");
                }

                entity.Name = dto.Name;
                entity.Code = dto.Code;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var entity = _context.Region.Find(id);
            if (entity != null)
            {
                // Проверяем, есть ли связанные города
                bool hasCities = _context.City.Any(c => c.RegionId == id);
                if (hasCities)
                {
                    throw new InvalidOperationException("Нельзя удалить регион, в котором есть города");
                }

                _context.Region.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}