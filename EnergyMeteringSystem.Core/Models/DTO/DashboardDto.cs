using System.Collections.Generic;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class DashboardDto
    {
        public int TotalObjects { get; set; }
        public int TotalMeters { get; set; }
        public int ReadingsToday { get; set; }
        public int ReadingsWeek { get; set; }
        public int ExpiredMeters { get; set; }
        public List<ChartPoint> ConsumptionChart { get; set; }
    }

    public class ChartPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }
}