using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class MeterReadingVerificationDto
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string SerialNumber { get; set; }
        public DateTime ReadingDate { get; set; }
        public decimal Value { get; set; }
        public decimal? PreviousValue { get; set; }
        public string EnteredBy { get; set; }
        public DateTime EnteredAt { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public bool IsSelected { get; set; } // для пакетной обработки
    }
}
