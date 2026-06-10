using System;

namespace EnergyMeteringSystem.Data.DTO
{
    public class VerificationDto
    {
        public int Id { get; set; }
        public string FullAddress { get; set; }
        public string SerialNumber { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public decimal? PreviousValue { get; set; }
        public string EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; }
        public int ReadingStatusId { get; set; }
        public string StatusName { get; set; }
        public bool IsSelected { get; set; }
    }
}