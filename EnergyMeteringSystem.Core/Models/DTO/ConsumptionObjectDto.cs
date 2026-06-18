namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ConsumptionObjectDto
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public int StreetId { get; set; }
        public string City { get; set; }
        public int CityId { get; set; }
        public string Region { get; set; }
        public int RegionId { get; set; }
        public string HouseNumber { get; set; }
        public string ApartmentNumber { get; set; }
        public int ObjectTypeId { get; set; }
        public string ObjectTypeName { get; set; }
        public decimal? TotalArea { get; set; }
        public int? ResidentCount { get; set; }

        // ✅ НОВОЕ
        public decimal? NormConsumption { get; set; }

        public string Address => $"{Region}, {City}, {Street}, д. {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $"/{ApartmentNumber}");

        public string ShortAddress => $"{City}, {Street}, {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $"-{ApartmentNumber}");

        public string FullInfo => $"{ShortAddress} ({ObjectTypeName})";
    }
}