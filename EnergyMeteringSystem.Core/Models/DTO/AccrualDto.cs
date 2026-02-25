using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class AccrualDto
    {
        public int Id { get; set; }
        public int ConsumptionObjectId { get; set; }
        public string Address { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal ConsumptionValue { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaidText => IsPaid ? "Оплачено" : "Не оплачено";
        public string PaidColor => IsPaid ? "Green" : "Red";
        public string PeriodText => $"{PeriodMonth:00}.{PeriodYear}";
    }

}
