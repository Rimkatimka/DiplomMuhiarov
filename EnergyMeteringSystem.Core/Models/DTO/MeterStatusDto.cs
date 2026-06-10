namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterStatusDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool CanAcceptReadings { get; set; }
    }
}
