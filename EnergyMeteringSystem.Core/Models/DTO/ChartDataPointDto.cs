using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ChartDataPointDto
    {
        public string MonthName { get; set; }
        public decimal Consumption { get; set; }
    }
}