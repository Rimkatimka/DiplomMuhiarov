using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class PaymentRegistrationDto
    {
        public int ConsumptionObjectId { get; set; }
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public int ReceivedByUserId { get; set; }
        public string ReceiptNumber { get; set; }
    }
}
