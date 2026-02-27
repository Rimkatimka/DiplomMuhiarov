using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterReadingInputDto
    {
        public int MeterId { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public int EnteredByUserId { get; set; }
        public int? RejectionReasonId { get; set; }
        public int ReadingStatusId { get; set; } = 1; // "Введено"
        public int TariffZone { get; set; } = 1;
        public string Comment { get; set; }
    }
}
