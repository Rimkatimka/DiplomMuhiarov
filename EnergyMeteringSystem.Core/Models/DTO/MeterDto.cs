using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string VerificationStatusText
        {
            get
            {
                if (!NextVerificationDate.HasValue) return "Не указана";
                if (NextVerificationDate.Value < DateTime.Today) return "Просрочена";
                if (NextVerificationDate.Value <= DateTime.Today.AddMonths(1)) return "Скоро";
                return "В норме";
            }
        }

        public string VerificationStatusColor
        {
            get
            {
                if (!NextVerificationDate.HasValue) return "Gray";
                if (NextVerificationDate.Value < DateTime.Today) return "Red";
                if (NextVerificationDate.Value <= DateTime.Today.AddMonths(1)) return "Orange";
                return "Green";
            }
        }
        public string StatusText => StatusName;
    }
}
