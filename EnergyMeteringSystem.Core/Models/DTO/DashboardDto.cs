using System.Collections.Generic;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class DashboardDto
    {
        public int TotalObjects { get; set; }
        public int TotalMeters { get; set; }
        public int ReadingsToday { get; set; }
        public int ReadingsWeek { get; set; }
        public decimal AccrualMonth { get; set; }
        public decimal PaymentMonth { get; set; }
        public int ExpiredMeters { get; set; }
        public List<DebtDto> TopDebtors { get; set; }
        public List<ChartPoint> ConsumptionChart { get; set; }
        public string AccrualText => $"{AccrualMonth:F0} ₽";
        public string PaymentText => $"{PaymentMonth:F0} ₽";
        public string DebtorsCountText => TopDebtors != null ? $"{TopDebtors.Count} должников" : "Нет должников";
    }

    public class ChartPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }
}
