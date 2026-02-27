namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Voltage { get; set; }
        public int MaxCurrent { get; set; }
        public string AccuracyClass { get; set; }
        public int DigitCount { get; set; }
        public int DecimalPlaces { get; set; }

        // Вычисляемые свойства
        public string DisplayName => $"{Name} ({Voltage}В, {MaxCurrent}А)";
        public string FullDescription => $"{Name}, {Voltage}В, {MaxCurrent}А, класс точности {AccuracyClass}";
    }
}
