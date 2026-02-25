using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterForReadingDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string MeterTypeName { get; set; }
        public decimal? LastReading { get; set; }
        public DateTime? LastReadingDate { get; set; }

        // Если нужно показывать статус
        public string StatusName { get; set; }

        // Вычисляемые свойства
        public string LastReadingInfo => LastReading.HasValue
            ? $"Последнее: {LastReading} от {LastReadingDate:dd.MM.yyyy}"
            : "Нет показаний";
    }
}
