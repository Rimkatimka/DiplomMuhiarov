using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Billing
{
    public class DebtViewModel : ViewModelBase
    {
        private readonly PaymentRepository _paymentRepository;
        private string _searchText;
        private DebtDto _selectedDebt;

        public ObservableCollection<DebtDto> Debts { get; set; }
        public ObservableCollection<DebtDto> FilteredDebts { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public DebtDto SelectedDebt
        {
            get => _selectedDebt;
            set => SetProperty(ref _selectedDebt, value);
        }

        public decimal TotalDebt => FilteredDebts?.Sum(d => d.DebtAmount) ?? 0;
        public int DebtorsCount => FilteredDebts?.Count ?? 0;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand MarkAsPaidCommand { get; }

        public DebtViewModel()
        {
            _paymentRepository = new PaymentRepository();

            Debts = new ObservableCollection<DebtDto>();
            FilteredDebts = new ObservableCollection<DebtDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            MarkAsPaidCommand = new RelayCommand(_ => MarkAsPaid(), _ => SelectedDebt != null);

            LoadData();
        }

        private void LoadData()
        {
            Debts.Clear();
            var list = _paymentRepository.GetDebtors();
            foreach (var debt in list)
                Debts.Add(debt);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredDebts.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Debts
                : new ObservableCollection<DebtDto>(
                    Debts.Where(d =>
                        d.Address.Contains(SearchText)));

            foreach (var debt in filtered)
                FilteredDebts.Add(debt);

            OnPropertyChanged(nameof(TotalDebt));
            OnPropertyChanged(nameof(DebtorsCount));
        }

        

        private void MarkAsPaid()
        {
            if (SelectedDebt == null) return;

            // Здесь можно открыть окно регистрации платежа
            System.Windows.MessageBox.Show(
                $"Отметить оплату для объекта: {SelectedDebt.Address}\nСумма долга: {SelectedDebt.DebtAmount:F2} ₽",
                "Оплата долга",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}
