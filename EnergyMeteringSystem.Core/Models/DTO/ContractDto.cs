using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ContractDto
    {
        public int Id { get; set; }
        public string ContractNumber { get; set; }
        public int ConsumptionObjectId { get; set; }
        public string Address { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ContractStatusId { get; set; }
        public string StatusName { get; set; }
        public int TariffId { get; set; }
        public string TariffName { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTime.Today;
        public string StatusText => IsActive ? "Активен" : "Расторгнут";
        public string PeriodText => $"{StartDate:dd.MM.yyyy} - {(EndDate?.ToString("dd.MM.yyyy") ?? "н.в.")}";
    }
}
