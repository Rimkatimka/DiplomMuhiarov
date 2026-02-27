using System;
using System.Collections.Generic;
using System.Linq;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public DateTime ActionTime { get; set; }
        public string ActionType { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Details { get; set; }
        public string ShortDetails
        {
            get
            {
                if (string.IsNullOrEmpty(Details))
                {
                    return ActionType;
                }

                // Убираем фигурные скобки и кавычки
                return Details.Replace("{", "").Replace("}", "").Replace("\"", "");
            }
        }
        public string DisplayDetails
        {
            get
            {
                if (ActionType == "LOGIN")
                {
                    return "Вход в систему";
                }

                if (ActionType == "LOGOUT")
                {
                    return "Выход из системы";
                }

                if (ActionType == "INSERT" && !string.IsNullOrEmpty(Details))
                {
                    try
                    {
                        Dictionary<string, object> dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Details);
                        return string.Join(", ", dict.Select(kv => $"{kv.Key}: {kv.Value}"));
                    }
                    catch
                    {
                        return Details;
                    }
                }
                return Details ?? ActionType;
            }
        }

        // Вычисляемые свойства
        public string TimeText => ActionTime.ToString("dd.MM.yyyy HH:mm:ss");
        public string ActionText => $"{ActionType} [{TableName}]";
        public string UserDisplay => UserName ?? "Система";
    }
}