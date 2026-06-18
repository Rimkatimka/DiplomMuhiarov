using EnergyMeteringSystem.Core.Models.DTO;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Helpers
{
    public static class AuditLogger
    {
        // Событие для передачи логов
        public static event Func<AuditLogDto, Task> OnLogAsync;

        // ✅ СТАРЫЙ МЕТОД ДЛЯ СОВМЕСТИМОСТИ (вызывает асинхронный)
        public static void Log(string actionType, string tableName, int recordId,
                               object oldValues = null, object newValues = null,
                               int? userId = null)
        {
            _ = LogAsync(actionType, tableName, recordId, oldValues, newValues, userId);
        }

        // ✅ НОВЫЙ АСИНХРОННЫЙ МЕТОД
        public static async Task LogAsync(string actionType, string tableName, int recordId,
                                          object oldValues = null, object newValues = null,
                                          int? userId = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AuditLogger.Log: {actionType} on {tableName}");

                var log = new AuditLogDto
                {
                    UserId = userId,
                    ActionTime = DateTime.Now,
                    ActionType = actionType,
                    TableName = tableName,
                    RecordId = recordId,
                    Details = GetDetails(oldValues, newValues)
                };

                // ✅ АСИНХРОННЫЙ ВЫЗОВ СОБЫТИЯ
                if (OnLogAsync != null)
                {
                    await OnLogAsync.Invoke(log);
                }

                System.Diagnostics.Debug.WriteLine($"AuditLogger.Log: Успешно сохранен");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка записи аудита: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            }
        }

        private static string GetDetails(object oldValues, object newValues)
        {
            if (oldValues != null && newValues != null)
            {
                return JsonConvert.SerializeObject(new { Old = oldValues, New = newValues });
            }
            if (newValues != null)
            {
                return JsonConvert.SerializeObject(newValues);
            }
            if (oldValues != null)
            {
                return JsonConvert.SerializeObject(oldValues);
            }
            return null;
        }
    }
}