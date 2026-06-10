using System;

public class MeterForReadingDto
{
    public int Id { get; set; }
    public string SerialNumber { get; set; }
    public string MeterTypeName { get; set; }
    public decimal? LastReading { get; set; }
    public DateTime? LastReadingDate { get; set; }
    public string StatusName { get; set; }

    // ✅ ДОБАВИТЬ для начального показания
    public decimal InitialReading { get; set; }
    public DateTime? InstallationDate { get; set; }

    public string LastReadingInfo => LastReading.HasValue
        ? $"Последнее: {LastReading} от {LastReadingDate:dd.MM.yyyy}"
        : (InitialReading > 0 ? $"Начальное: {InitialReading}" : "Нет показаний");
}