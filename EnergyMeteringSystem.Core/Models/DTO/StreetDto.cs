namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class StreetDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string PostalCode { get; set; }
    }
}
