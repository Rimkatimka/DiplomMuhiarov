using System;
using System.Collections.Generic;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class MeterRepository : IMeterRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public MeterRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public List<MeterDto> GetByObjectId(int objectId)
        {
            try
            {
                if (objectId <= 0) return new List<MeterDto>();

                var meters = _context.Meter
                    .Where(m => m.ConsumptionObjectId == objectId)
                    .Select(m => new MeterDto
                    {
                        Id = m.Id,
                        SerialNumber = m.SerialNumber,
                        MeterTypeId = m.MeterTypeId,
                        MeterTypeName = m.MeterType.Name,
                        StatusId = m.MeterStatusId,
                        StatusName = m.MeterStatus.Name,
                        InstallationDate = m.InstallationDate,
                        VerificationDate = m.VerificationDate,
                        NextVerificationDate = m.NextVerificationDate,
                        InitialReading = m.InitialReading,
                        ConsumptionObjectId = m.ConsumptionObjectId,
                        ServiceLifeYears = m.ServiceLifeYears   // ← добавить!
                    })
                    .ToList();

                return meters;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetByObjectId: {ex.Message}");
                return new List<MeterDto>();
            }
        }
        public List<MeterDto> GetAll()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("MeterRepository.GetAll() начат");

                List<Meter> meters = _context.Meter
                    .Include("MeterType")
                    .Include("MeterStatus")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Загружено {meters.Count} счетчиков из БД");

                List<MeterDto> result = meters.Select(m => new MeterDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeId = m.MeterTypeId,
                    MeterTypeName = m.MeterType?.Name ?? "Неизвестно",
                    StatusId = m.MeterStatusId,
                    StatusName = m.MeterStatus?.Name ?? "Неизвестно",
                    InstallationDate = m.InstallationDate,
                    VerificationDate = m.VerificationDate,
                    NextVerificationDate = m.NextVerificationDate,
                    InitialReading = m.InitialReading
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"GetAll возвращает {result.Count} счетчиков");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в GetAll: {ex.Message}");
                return [];
            }
        }

        private string GetMeterTypeName(int typeId)
        {
            MeterType type = _context.MeterType.Find(typeId);
            return type?.Name ?? "Неизвестно";
        }

        private string GetMeterStatusName(int statusId)
        {
            MeterStatus status = _context.MeterStatus.Find(statusId);
            return status?.Name ?? "Неизвестно";
        }
        public void Update(MeterDto dto)
        {
            try
            {
                var entity = _context.Meter.Find(dto.Id);
                if (entity != null)
                {
                    entity.SerialNumber = dto.SerialNumber;
                    entity.MeterTypeId = dto.MeterTypeId;
                    entity.ConsumptionObjectId = dto.ConsumptionObjectId;
                    entity.InstallationDate = dto.InstallationDate;
                    entity.InitialReading = dto.InitialReading;
                    entity.VerificationDate = dto.VerificationDate;
                    entity.MeterStatusId = dto.StatusId;
                    entity.ServiceLifeYears = dto.ServiceLifeYears;   // ← добавить!

                    if (dto.VerificationDate.HasValue)
                    {
                        entity.NextVerificationDate = dto.VerificationDate.Value.AddYears(6);
                    }

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в Update: {ex.Message}");
                throw;
            }
        }

        public void Add(MeterDto dto)
        {
            Meter entity = new()
            {
                SerialNumber = dto.SerialNumber,
                MeterTypeId = dto.MeterTypeId,
                ConsumptionObjectId = dto.ConsumptionObjectId,
                InstallationDate = dto.InstallationDate,
                InitialReading = dto.InitialReading,
                VerificationDate = dto.VerificationDate,
                MeterStatusId = dto.StatusId
            };

            _ = _context.Meter.Add(entity);
            _ = _context.SaveChanges();
        }
        public MeterDto GetById(int id)
        {
            var meter = _context.Meter
                .Include("MeterType")
                .Include("MeterStatus")
                .FirstOrDefault(m => m.Id == id);

            if (meter == null) return null;

            return new MeterDto
            {
                Id = meter.Id,
                SerialNumber = meter.SerialNumber,
                MeterTypeId = meter.MeterTypeId,
                MeterTypeName = meter.MeterType?.Name,
                StatusId = meter.MeterStatusId,
                StatusName = meter.MeterStatus?.Name,
                InstallationDate = meter.InstallationDate,
                VerificationDate = meter.VerificationDate,
                NextVerificationDate = meter.NextVerificationDate,
                InitialReading = meter.InitialReading,
                ConsumptionObjectId = meter.ConsumptionObjectId,
                ServiceLifeYears = meter.ServiceLifeYears
            };
        }
        public void Delete(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"MeterRepository.Delete: удаление счетчика ID={id}");

                Meter entity = _context.Meter.Find(id);
                if (entity != null)
                {
                    _ = _context.Meter.Remove(entity);
                    _ = _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("MeterRepository.Delete: удаление выполнено успешно");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в MeterRepository.Delete: {ex.Message}");
                throw;
            }
        }
    }
}
