using System;
using System.Collections.Generic;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class RegionAnalyticsDto
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public decimal TotalConsumption { get; set; }
        public int ObjectsCount { get; set; }
        public decimal AveragePerObject { get; set; }
        public List<CityAnalyticsDto> Cities { get; set; }

        public RegionAnalyticsDto()
        {
            Cities = new List<CityAnalyticsDto>();
        }
    }
}