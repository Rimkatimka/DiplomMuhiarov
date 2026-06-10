using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class AuditRepository : BaseRepository, IAuditRepository
    {
        // Синхронный метод (оставляем)
        public List<AuditLogDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ АСИНХРОННЫЙ GetAll
        public async Task<List<AuditLogDto>> GetAllAsync()
        {
            try
            {
                var logs = await Query<AuditLog>()
                    .OrderByDescending(a => a.ActionTime)
                    .Take(1000)
                    .ToListAsync();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User?.FullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetAll error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        // ✅ АСИНХРОННЫЙ GetByDate
        public async Task<List<AuditLogDto>> GetByDateAsync(DateTime from, DateTime to)
        {
            try
            {
                var endDate = to.AddDays(1);
                var logs = await Query<AuditLog>()
                    .Where(a => a.ActionTime >= from && a.ActionTime < endDate)
                    .OrderByDescending(a => a.ActionTime)
                    .Take(1000)
                    .ToListAsync();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User?.FullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetByDate error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        // Синхронный GetByDate (для совместимости)
        public List<AuditLogDto> GetByDate(DateTime from, DateTime to)
        {
            return GetByDateAsync(from, to).Result;
        }

        // ... остальные методы без изменений ...

        public List<AuditLogDto> GetByUser(int userId)
        {
            try
            {
                var logs = Query<AuditLog>()
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.ActionTime)
                    .Take(500)
                    .ToList();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User?.FullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetByUser error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        public List<AuditLogDto> GetByTable(string tableName)
        {
            try
            {
                var logs = Query<AuditLog>()
                    .Where(a => a.TableName == tableName)
                    .OrderByDescending(a => a.ActionTime)
                    .Take(500)
                    .ToList();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User?.FullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetails(a)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetByTable error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        public void Log(AuditLogDto dto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.Log: {dto.ActionType} на {dto.TableName}, ID={dto.RecordId}");

                var entity = new AuditLog
                {
                    UserId = dto.UserId,
                    ActionTime = DateTime.Now,
                    ActionType = dto.ActionType,
                    TableName = dto.TableName,
                    RecordId = dto.RecordId,
                    NewValuesJson = dto.Details,
                    OldValuesJson = dto.Details,
                    IpAddress = "Local"
                };

                _context.AuditLog.Add(entity);
                _context.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"AuditRepository.Log: сохранено успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.Log error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private string GetDetails(AuditLog log)
        {
            if (!string.IsNullOrEmpty(log.OldValuesJson) && !string.IsNullOrEmpty(log.NewValuesJson))
                return $"Было: {log.OldValuesJson}, Стало: {log.NewValuesJson}";
            else if (!string.IsNullOrEmpty(log.NewValuesJson))
                return log.NewValuesJson;
            else if (!string.IsNullOrEmpty(log.OldValuesJson))
                return log.OldValuesJson;
            else
                return log.ActionType;
        }
    }
}