using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterReadingDto
    {
        public int Id { get; set; }
        public int MeterId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public int ReadingStatusId { get; set; }  // ← Добавить!
        public string StatusName { get; set; }
        public string EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; }

        // Вычисляемое свойство для текста статуса
        public string StatusText => ReadingStatusId switch
        {
            1 => "Введено",
            2 => "Подтверждено",
            3 => "Отклонено",
            _ => "Неизвестно",
        };

        // Для отображения цвета статуса в UI
        public string StatusColor => ReadingStatusId switch
        {
            1 => "Orange",
            2 => "Green",
            3 => "Red",
            _ => "Gray",
        };
    }
}
