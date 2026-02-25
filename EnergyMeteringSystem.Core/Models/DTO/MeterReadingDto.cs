using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string StatusText
        {
            get
            {
                switch (ReadingStatusId)
                {
                    case 1: return "Введено";
                    case 2: return "Подтверждено";
                    case 3: return "Отклонено";
                    default: return "Неизвестно";
                }
            }
        }

        // Для отображения цвета статуса в UI
        public string StatusColor
        {
            get
            {
                switch (ReadingStatusId)
                {
                    case 1: return "Orange";
                    case 2: return "Green";
                    case 3: return "Red";
                    default: return "Gray";
                }
            }
        }
    }
}
