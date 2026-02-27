using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class TariffDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TariffTypeId { get; set; }
        public string TariffTypeName { get; set; }
        public int ZoneNumber { get; set; }
        public decimal PricePerUnit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTime.Today;
        public string StatusText => IsActive ? "Активен" : "Неактивен";
        public string ZoneName => ZoneNumber == 1 ? "День" : "Ночь";
        public string PriceText => $"{PricePerUnit:F2} ₽/кВт";
        public string PeriodText => $"{StartDate:dd.MM.yyyy} - {EndDate?.ToString("dd.MM.yyyy") ?? "бессрочно"}";
    }
}
