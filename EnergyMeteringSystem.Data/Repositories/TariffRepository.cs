using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class TariffRepository : ITariffRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public TariffRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<TariffDto> GetAll()
        {
            return _context.Tariff
                .Include("TariffType")
                .OrderBy(t => t.TariffType.Name)
                .ThenBy(t => t.ZoneNumber)
                .ThenByDescending(t => t.StartDate)
                .Select(t => new TariffDto
                {
                    Id = t.Id,
                    TariffTypeId = t.TariffTypeId,
                    TariffTypeName = t.TariffType.Name,
                    ZoneNumber = t.ZoneNumber,
                    ZoneName = GetZoneName(t.ZoneNumber),
                    PricePerUnit = t.PricePerUnit,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .ToList();
        }

        public List<TariffDto> GetActive()
        {
            var today = DateTime.Today;
            return _context.Tariff
                .Include("TariffType")
                .Where(t => t.StartDate <= today &&
                           (t.EndDate == null || t.EndDate > today))
                .Select(t => new TariffDto
                {
                    Id = t.Id,
                    TariffTypeId = t.TariffTypeId,
                    TariffTypeName = t.TariffType.Name,
                    ZoneNumber = t.ZoneNumber,
                    ZoneName = GetZoneName(t.ZoneNumber),
                    PricePerUnit = t.PricePerUnit,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .ToList();
        }

        public TariffDto GetById(int id)
        {
            var t = _context.Tariff
                .Include("TariffType")
                .FirstOrDefault(x => x.Id == id);

            if (t == null) return null;

            return new TariffDto
            {
                Id = t.Id,
                TariffTypeId = t.TariffTypeId,
                TariffTypeName = t.TariffType.Name,
                ZoneNumber = t.ZoneNumber,
                ZoneName = GetZoneName(t.ZoneNumber),
                PricePerUnit = t.PricePerUnit,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            };
        }

        public TariffDto GetCurrentByType(int tariffTypeId)
        {
            var today = DateTime.Today;
            var t = _context.Tariff
                .Where(tar => tar.TariffTypeId == tariffTypeId &&
                              tar.StartDate <= today &&
                              (tar.EndDate == null || tar.EndDate > today))
                .OrderBy(tar => tar.ZoneNumber)
                .FirstOrDefault();

            if (t == null) return null;

            return new TariffDto
            {
                Id = t.Id,
                TariffTypeId = t.TariffTypeId,
                TariffTypeName = t.TariffType.Name,
                ZoneNumber = t.ZoneNumber,
                ZoneName = GetZoneName(t.ZoneNumber),
                PricePerUnit = t.PricePerUnit,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            };
        }

        public void Add(TariffDto dto)
        {
            // Проверяем, нет ли уже активного тарифа на эту дату
            var existing = _context.Tariff
                .FirstOrDefault(t => t.TariffTypeId == dto.TariffTypeId &&
                                    t.ZoneNumber == dto.ZoneNumber &&
                                    t.StartDate <= dto.StartDate &&
                                    (t.EndDate == null || t.EndDate > dto.StartDate));

            if (existing != null)
            {
                existing.EndDate = dto.StartDate.AddDays(-1);
            }

            var entity = new Tariff
            {
                TariffTypeId = dto.TariffTypeId,
                ZoneNumber = dto.ZoneNumber,
                PricePerUnit = dto.PricePerUnit,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UnitId = 1 // кВт·ч
            };

            _context.Tariff.Add(entity);
            _context.SaveChanges();
        }

        public void Update(TariffDto dto)
        {
            var entity = _context.Tariff.Find(dto.Id);
            if (entity != null)
            {
                entity.PricePerUnit = dto.PricePerUnit;
                entity.EndDate = dto.EndDate;
                _context.SaveChanges();
            }
        }

        public void Deactivate(int id, DateTime endDate)
        {
            var entity = _context.Tariff.Find(id);
            if (entity != null)
            {
                entity.EndDate = endDate;
                _context.SaveChanges();
            }
        }

        private string GetZoneName(int zoneNumber)
        {
            return zoneNumber == 1 ? "День" : "Ночь";
        }
    }
}
