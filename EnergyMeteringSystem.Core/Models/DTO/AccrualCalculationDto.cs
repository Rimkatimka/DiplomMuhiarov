namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class AccrualCalculationDto
    {
        public int ConsumptionObjectId { get; set; }
        public string Address { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal TotalAmount { get; set; }
        public int ReadingsCount { get; set; }
        public bool HasExistingAccrual { get; set; }
    }
}
