public class MeterDataHistoryDto
{
    public int Id { get; set; }
    public int MeterId { get; set; }
    public int PeriodYear { get; set; }
    public int PeriodMonth { get; set; }
    public decimal Value { get; set; }
    public string SourceType { get; set; } // "Reading" - показание, "Correction" - корректировка
    public string PeriodText => $"{PeriodMonth:00}.{PeriodYear}";
}