namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ContractStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AllowsBilling { get; set; }
    }
}
