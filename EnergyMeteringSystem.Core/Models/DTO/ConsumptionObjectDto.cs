using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ConsumptionObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string ApartmentNumber { get; set; }
        public string ObjectTypeName { get; set; }
        public decimal? TotalArea { get; set; }
        public int? ResidentCount { get; set; }
        public int StreetId { get; set; }
        public int ObjectTypeId { get; set; }

        public string Address => $"{Street}, д. {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $", кв. {ApartmentNumber}");

        public string ShortAddress => $"{Street}, {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $"-{ApartmentNumber}");

        public string FullInfo => $"{ShortAddress} ({ObjectTypeName})";
    }
}