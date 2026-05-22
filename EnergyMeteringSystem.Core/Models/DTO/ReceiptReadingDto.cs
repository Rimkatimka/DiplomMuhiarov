public class ReceiptReadingDto
{
    public string SerialNumber { get; set; }
    public decimal CurrentReading { get; set; }
    public decimal PreviousReading { get; set; }
    public decimal Consumption { get; set; }
    public decimal Amount { get; set; }
}