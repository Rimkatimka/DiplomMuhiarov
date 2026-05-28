using System;
using System.Collections.Generic;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class CityAnalyticsDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public decimal TotalConsumption { get; set; }
        public int ObjectsCount { get; set; }
        public decimal AveragePerObject { get; set; }
        public List<StreetAnalyticsDto> Streets { get; set; }

        public CityAnalyticsDto()
        {
            Streets = new List<StreetAnalyticsDto>();
        }
    }
}