using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ObjectAnalyticsDto
    {
        public int ObjectId { get; set; }
        public string Address { get; set; }
        public string HouseNumber { get; set; }
        public string ApartmentNumber { get; set; }
        public string ObjectType { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
    }
}