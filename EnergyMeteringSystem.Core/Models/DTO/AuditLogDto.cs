using System;

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

        // Вычисляемые свойства
        public string TimeText => ActionTime.ToString("dd.MM.yyyy HH:mm:ss");
        public string ActionText => $"{ActionType} [{TableName}]";
        public string UserDisplay => UserName ?? "Система";
    }
}