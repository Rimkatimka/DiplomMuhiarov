using System;
using System.Collections.ObjectModel;
using System.Linq;
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
                _ = SetProperty(ref _searchText, value);
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
            System.Diagnostics.Debug.WriteLine("DebtViewModel: конструктор начат");

            _paymentRepository = new PaymentRepository();

            Debts = [];
            FilteredDebts = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            MarkAsPaidCommand = new RelayCommand(_ => MarkAsPaid(), _ => SelectedDebt != null);

            LoadData();

            System.Diagnostics.Debug.WriteLine("DebtViewModel: конструктор завершен");
        }

        private void LoadData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== DebtViewModel.LoadData: начало ===");

                Debts.Clear();
                System.Collections.Generic.List<DebtDto> list = _paymentRepository.GetDebtors();

                System.Diagnostics.Debug.WriteLine($"Загружено {list.Count} должников из репозитория");

                foreach (DebtDto debt in list)
                {
                    Debts.Add(debt);
                    System.Diagnostics.Debug.WriteLine($"  - {debt.Address}: {debt.DebtAmount} ₽");
                }

                ApplyFilter();

                System.Diagnostics.Debug.WriteLine($"После ApplyFilter: {FilteredDebts.Count} должников");
                System.Diagnostics.Debug.WriteLine("=== DebtViewModel.LoadData: конец ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в LoadData: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            try
            {
                FilteredDebts.Clear();

                ObservableCollection<DebtDto> filtered = string.IsNullOrWhiteSpace(SearchText)
                    ? Debts
                    : [.. Debts.Where(d => d.Address.Contains(SearchText))];

                foreach (DebtDto debt in filtered)
                {
                    FilteredDebts.Add(debt);
                }

                OnPropertyChanged(nameof(TotalDebt));
                OnPropertyChanged(nameof(DebtorsCount));

                System.Diagnostics.Debug.WriteLine($"ApplyFilter: {FilteredDebts.Count} должников после фильтра");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в ApplyFilter: {ex.Message}");
            }
        }

        private void MarkAsPaid()
        {
            if (SelectedDebt == null)
            {
                return;
            }

            _ = System.Windows.MessageBox.Show(
                $"Отметить оплату для объекта: {SelectedDebt.Address}\nСумма долга: {SelectedDebt.DebtAmount:F2} ₽",
                "Оплата долга",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}