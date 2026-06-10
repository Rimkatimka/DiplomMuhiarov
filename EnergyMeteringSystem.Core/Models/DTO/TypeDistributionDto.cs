using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class TypeDistributionDto
    {
        public string TypeName { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
    }
}