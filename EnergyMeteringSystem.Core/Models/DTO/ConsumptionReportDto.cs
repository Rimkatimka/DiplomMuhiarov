using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ConsumptionReportDto
    {
        public int ObjectId { get; set; }
        public string Address { get; set; }
        public string MeterSerial { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartValue { get; set; }
        public decimal EndValue { get; set; }
        public decimal Consumption { get; set; }
        public string PeriodText => $"{StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy}";
        public string ConsumptionText => $"{Consumption:F2} кВт";
    }
}
