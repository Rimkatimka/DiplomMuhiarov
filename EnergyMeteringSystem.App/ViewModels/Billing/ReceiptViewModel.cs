using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using System.Collections.ObjectModel;

namespace EnergyMeteringSystem.App.ViewModels.Billing
{
    public class ReceiptViewModel : ViewModelBase
    {
        public string ReceiptNumber { get; set; }
        public string PaymentDate { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string AccountNumber { get; set; }
        public string PeriodText { get; set; }
        public string TariffText { get; set; }
        public decimal AccrualAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public string DebtColor => DebtAmount > 0 ? "Red" : "Green";

        public ObservableCollection<ReceiptReadingDto> Readings { get; set; }

        public ReceiptViewModel(ConsumptionObjectDto obj, int year, int month)
        {
            Readings = new ObservableCollection<ReceiptReadingDto>();
            // TODO: заполнить данными
        }
    }

    public class ReceiptReadingDto
    {
        public string SerialNumber { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal PreviousReading { get; set; }
        public decimal Consumption { get; set; }
        public decimal Amount { get; set; }
    }
}