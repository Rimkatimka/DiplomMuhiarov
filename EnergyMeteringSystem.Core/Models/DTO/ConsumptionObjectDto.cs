namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ConsumptionObjectDto
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public int StreetId { get; set; }
        public string City { get; set; }           // ← добавить
        public int CityId { get; set; }            // ← добавить
        public string Region { get; set; }         // ← добавить
        public int RegionId { get; set; }          // ← добавить
        public string HouseNumber { get; set; }
        public string ApartmentNumber { get; set; }
        public int ObjectTypeId { get; set; }
        public string ObjectTypeName { get; set; }
        public decimal? TotalArea { get; set; }
        public int? ResidentCount { get; set; }

        // Полный адрес с городом
        public string Address => $"{City}, {Street}, д. {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $", кв. {ApartmentNumber}");

        public string ShortAddress => $"{City}, {Street}, {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $"-{ApartmentNumber}");

        public string FullAddress => $"{Region}, {City}, {Street}, {HouseNumber}" +
            (string.IsNullOrEmpty(ApartmentNumber) ? "" : $"/{ApartmentNumber}");

        public string FullInfo => $"{ShortAddress} ({ObjectTypeName})";
    }
}