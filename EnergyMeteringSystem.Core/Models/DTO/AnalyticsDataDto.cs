using System;
using System.Collections.Generic;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class AnalyticsDataDto
    {
        public List<TopObjectDto> TopObjects { get; set; }
        public List<TypeDistributionDto> TypeDistribution { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal AverageConsumption { get; set; }
        public decimal MaxConsumption { get; set; }

        public AnalyticsDataDto()
        {
            TopObjects = new List<TopObjectDto>();
            TypeDistribution = new List<TypeDistributionDto>();
        }
    }
}