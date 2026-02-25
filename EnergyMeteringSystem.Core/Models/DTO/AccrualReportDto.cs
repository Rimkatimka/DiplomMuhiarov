using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class AccrualReportDto
    {
        public int ObjectId { get; set; }
        public string Address { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal AccrualAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public string MonthText
        {
            get
            {
                string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн",
                            "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
                return months[PeriodMonth - 1];
            }
        }

        public string AccrualText => $"{AccrualAmount:F2} ₽";
        public string PaidText => $"{PaidAmount:F2} ₽";
        public string DebtText => $"{DebtAmount:F2} ₽";
    }
}
