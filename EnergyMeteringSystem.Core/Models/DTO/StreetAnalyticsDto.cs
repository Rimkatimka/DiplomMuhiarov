using System;
using System.Collections.Generic;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class StreetAnalyticsDto
    {
        public int StreetId { get; set; }
        public string StreetName { get; set; }
        public decimal TotalConsumption { get; set; }
        public int ObjectsCount { get; set; }
        public decimal AveragePerObject { get; set; }
        public List<ObjectAnalyticsDto> Objects { get; set; }

        public StreetAnalyticsDto()
        {
            Objects = new List<ObjectAnalyticsDto>();
        }
    }
}