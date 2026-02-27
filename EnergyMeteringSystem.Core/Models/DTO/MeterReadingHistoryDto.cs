using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterReadingHistoryDto
    {
        public int Id { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public decimal? Consumption { get; set; } // разница с предыдущим
        public string StatusName { get; set; }
        public string EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; }
    }
}
