using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int ConsumptionObjectId { get; set; }
        public string Address { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethodName { get; set; }
        public string ReceivedBy { get; set; }
        public string ReceiptNumber { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public string DateText => PaymentDate.ToString("dd.MM.yyyy HH:mm");
        public string PeriodText => $"{PeriodMonth:00}.{PeriodYear}";
    }
}
