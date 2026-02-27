using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterDto
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public int MeterTypeId { get; set; }
        public int ConsumptionObjectId { get; set; }
        public string MeterTypeName { get; set; }
        public string StatusName { get; set; }
        public DateTime InstallationDate { get; set; }
        public DateTime? VerificationDate { get; set; }
        public DateTime? NextVerificationDate { get; set; }
        public decimal InitialReading { get; set; }
        public int StatusId { get; set; }

        public string VerificationStatusText => !NextVerificationDate.HasValue
                    ? "Не указана"
                    : NextVerificationDate.Value < DateTime.Today
                    ? "Просрочена"
                    : NextVerificationDate.Value <= DateTime.Today.AddMonths(1) ? "Скоро" : "В норме";

        public string VerificationStatusColor => !NextVerificationDate.HasValue
                    ? "Gray"
                    : NextVerificationDate.Value < DateTime.Today
                    ? "Red"
                    : NextVerificationDate.Value <= DateTime.Today.AddMonths(1) ? "Orange" : "Green";
        public string StatusText => StatusName;
    }
}
