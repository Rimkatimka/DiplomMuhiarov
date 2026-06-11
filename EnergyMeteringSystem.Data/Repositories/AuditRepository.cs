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
        // Константы для ограничений
        private const int DEFAULT_TAKE_COUNT = 1000;
        private const int USER_TAKE_COUNT = 500;

        // Синхронный метод (оставляем для совместимости)
        public List<AuditLogDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ АСИНХРОННЫЙ GetAll
        public async Task<List<AuditLogDto>> GetAllAsync()
        {
            try
            {
                var logs = await Query<AuditLog>()
                    .OrderByDescending(a => a.ActionTime)
                    .Take(DEFAULT_TAKE_COUNT)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.ActionTime,
                        a.ActionType,
                        a.TableName,
                        a.RecordId,
                        a.OldValuesJson,
                        a.NewValuesJson,
                        UserFullName = a.User.FullName
                    })
                    .ToListAsync();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserFullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetailsFromJson(a.OldValuesJson, a.NewValuesJson, a.ActionType)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetAll error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ АСИНХРОННЫЙ GetByDate
        public async Task<List<AuditLogDto>> GetByDateAsync(DateTime from, DateTime to)
        {
            try
            {
                var endDate = to.AddDays(1);

                var logs = await Query<AuditLog>()
                    .Where(a => a.ActionTime >= from && a.ActionTime < endDate)
                    .OrderByDescending(a => a.ActionTime)
                    .Take(DEFAULT_TAKE_COUNT)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.ActionTime,
                        a.ActionType,
                        a.TableName,
                        a.RecordId,
                        a.OldValuesJson,
                        a.NewValuesJson,
                        UserFullName = a.User.FullName
                    })
                    .ToListAsync();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserFullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetailsFromJson(a.OldValuesJson, a.NewValuesJson, a.ActionType)
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

        // ✅ ОПТИМИЗИРОВАННЫЙ GetByUser (асинхронный)
        public async Task<List<AuditLogDto>> GetByUserAsync(int userId)
        {
            try
            {
                var logs = await Query<AuditLog>()
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.ActionTime)
                    .Take(USER_TAKE_COUNT)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.ActionTime,
                        a.ActionType,
                        a.TableName,
                        a.RecordId,
                        a.OldValuesJson,
                        a.NewValuesJson,
                        UserFullName = a.User.FullName
                    })
                    .ToListAsync();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserFullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetailsFromJson(a.OldValuesJson, a.NewValuesJson, a.ActionType)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetByUser error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        // Синхронный GetByUser (для совместимости)
        public List<AuditLogDto> GetByUser(int userId)
        {
            return GetByUserAsync(userId).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetByTable (асинхронный)
        public async Task<List<AuditLogDto>> GetByTableAsync(string tableName)
        {
            try
            {
                var logs = await Query<AuditLog>()
                    .Where(a => a.TableName == tableName)
                    .OrderByDescending(a => a.ActionTime)
                    .Take(USER_TAKE_COUNT)
                    .Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.ActionTime,
                        a.ActionType,
                        a.TableName,
                        a.RecordId,
                        a.OldValuesJson,
                        a.NewValuesJson,
                        UserFullName = a.User.FullName
                    })
                    .ToListAsync();

                return logs.Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.UserFullName ?? "Система",
                    ActionTime = a.ActionTime,
                    ActionType = a.ActionType,
                    TableName = a.TableName,
                    RecordId = a.RecordId,
                    Details = GetDetailsFromJson(a.OldValuesJson, a.NewValuesJson, a.ActionType)
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetByTable error: {ex.Message}");
                return new List<AuditLogDto>();
            }
        }

        // Синхронный GetByTable (для совместимости)
        public List<AuditLogDto> GetByTable(string tableName)
        {
            return GetByTableAsync(tableName).Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ Log (асинхронный)
        public async Task LogAsync(AuditLogDto dto)
        {
            if (dto == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.Log: {dto.ActionType} на {dto.TableName}, ID={dto.RecordId}");

                var entity = new AuditLog
                {
                    UserId = dto.UserId,
                    ActionTime = DateTime.Now,
                    ActionType = dto.ActionType?.Length > 50 ? dto.ActionType.Substring(0, 50) : dto.ActionType,
                    TableName = dto.TableName?.Length > 50 ? dto.TableName.Substring(0, 50) : dto.TableName,
                    RecordId = dto.RecordId,
                    NewValuesJson = dto.Details?.Length > 4000 ? dto.Details.Substring(0, 4000) : dto.Details,
                    OldValuesJson = dto.Details?.Length > 4000 ? dto.Details.Substring(0, 4000) : dto.Details,
                    IpAddress = "Local"
                };

                _context.AuditLog.Add(entity);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"AuditRepository.Log: сохранено успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.Log error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Синхронный Log (для совместимости)
        public void Log(AuditLogDto dto)
        {
            LogAsync(dto).Wait();
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ метод получения деталей (без создания объекта Log)
        private string GetDetailsFromJson(string oldJson, string newJson, string actionType)
        {
            if (!string.IsNullOrEmpty(oldJson) && !string.IsNullOrEmpty(newJson))
                return $"Было: {TruncateJson(oldJson)}, Стало: {TruncateJson(newJson)}";
            else if (!string.IsNullOrEmpty(newJson))
                return TruncateJson(newJson);
            else if (!string.IsNullOrEmpty(oldJson))
                return TruncateJson(oldJson);
            else
                return actionType ?? "Unknown";
        }

        // Обрезка слишком длинных JSON строк
        private string TruncateJson(string json, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(json)) return json;
            return json.Length > maxLength ? json.Substring(0, maxLength) + "..." : json;
        }

        // Старый метод GetDetails (оставляем для совместимости, но используем новый)
        private string GetDetails(AuditLog log)
        {
            return GetDetailsFromJson(log.OldValuesJson, log.NewValuesJson, log.ActionType);
        }

        // ✅ НОВЫЙ МЕТОД - получение количества записей (полезно для пагинации)
        public async Task<int> GetCountByDateAsync(DateTime from, DateTime to)
        {
            try
            {
                var endDate = to.AddDays(1);
                return await Query<AuditLog>()
                    .Where(a => a.ActionTime >= from && a.ActionTime < endDate)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditRepository.GetCountByDate error: {ex.Message}");
                return 0;
            }
        }

        // ✅ НОВЫЙ МЕТОД - удаление старых записей (для очистки логов)
        public async Task<int> DeleteOldLogsAsync(DateTime olderThan)
        {
            try
            {
                var oldLogs = await Query<AuditLog>()
                    .Where(a => a.ActionTime < olderThan)
                    .Take(10000)
                    .ToListAsync();

                if (!oldLogs.Any()) return 0;

                _context.AuditLog.RemoveRange(oldLogs);
                var deletedCount = await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Удалено {deletedCount} старых записей аудита");
                return deletedCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DeleteOldLogsAsync error: {ex.Message}");
                return 0;
            }
        }
    }
}