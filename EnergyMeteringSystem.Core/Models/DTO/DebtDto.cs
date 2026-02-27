using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class DebtDto
    {
        public int ConsumptionObjectId { get; set; }
        public string Address { get; set; }
        public decimal DebtAmount { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public int MonthsOverdue { get; set; }
        public string DebtAmountText => $"{DebtAmount:F2} ₽";
        public string PeriodText => $"{PeriodMonth:00}.{PeriodYear}";
        public string OverdueText => $"{MonthsOverdue} мес.";
    }
}
