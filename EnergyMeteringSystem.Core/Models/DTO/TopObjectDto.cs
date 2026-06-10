using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class TopObjectDto
    {
        public int Rank { get; set; }
        public int ObjectId { get; set; }
        public string Address { get; set; }
        public string ObjectType { get; set; }
        public decimal Consumption { get; set; }
        public decimal Percentage { get; set; }
    }
}