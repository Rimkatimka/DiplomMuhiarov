namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ObjectTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? NormConsumption { get; set; }
    }
}
