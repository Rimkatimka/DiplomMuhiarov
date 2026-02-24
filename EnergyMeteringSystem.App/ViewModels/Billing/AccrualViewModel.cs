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
using EnergyMeteringSystem.Services.Calculation;

namespace EnergyMeteringSystem.App.ViewModels.Billing
{
    public class AccrualViewModel : ViewModelBase
    {
        private readonly AccrualRepository _accrualRepository;
        private readonly CalculationService _calculationService;

        private int _selectedYear;
        private int _selectedMonth;
        private AccrualCalculationDto _selectedItem;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<AccrualCalculationDto> Calculations { get; set; }
        public ObservableCollection<AccrualDto> ExistingAccruals { get; set; }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                SetProperty(ref _selectedYear, value);
                LoadData();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                SetProperty(ref _selectedMonth, value);
                LoadData();
            }
        }

        public AccrualCalculationDto SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public decimal TotalAmount => Calculations?.Sum(c => c.TotalAmount) ?? 0;

        public RelayCommand CalculateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public AccrualViewModel()
        {
            _accrualRepository = new AccrualRepository();
            _calculationService = new CalculationService();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            Calculations = new ObservableCollection<AccrualCalculationDto>();
            ExistingAccruals = new ObservableCollection<AccrualDto>();

            CalculateCommand = new RelayCommand(_ => Calculate(), _ => CanCalculate());
            SaveCommand = new RelayCommand(_ => SaveSelected(), _ => CanSave());
            RefreshCommand = new RelayCommand(_ => LoadData());

            InitializeYearsAndMonths();
        }

        private void InitializeYearsAndMonths()
        {
            for (int i = 2020; i <= DateTime.Today.Year; i++)
                Years.Add(i);

            Months.Add("Январь"); Months.Add("Февраль"); Months.Add("Март");
            Months.Add("Апрель"); Months.Add("Май"); Months.Add("Июнь");
            Months.Add("Июль"); Months.Add("Август"); Months.Add("Сентябрь");
            Months.Add("Октябрь"); Months.Add("Ноябрь"); Months.Add("Декабрь");

            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;
        }

        private bool CanCalculate()
        {
            return _selectedYear > 0 && _selectedMonth > 0;
        }

        private void Calculate()
        {
            var results = _calculationService.CalculateForPeriod(_selectedYear, _selectedMonth);
            Calculations.Clear();
            foreach (var item in results)
                Calculations.Add(item);

            OnPropertyChanged(nameof(TotalAmount));
        }

        private bool CanSave()
        {
            return SelectedItem != null && !SelectedItem.HasExistingAccrual;
        }

        private void SaveSelected()
        {
            if (SelectedItem == null) return;

            var dto = new AccrualDto
            {
                ConsumptionObjectId = SelectedItem.ConsumptionObjectId,
                PeriodYear = _selectedYear,
                PeriodMonth = _selectedMonth,
                ConsumptionValue = SelectedItem.TotalConsumption,
                Amount = SelectedItem.TotalAmount,
                IsPaid = false
            };

            _accrualRepository.Add(dto);
            SelectedItem.HasExistingAccrual = true;
            LoadExistingAccruals();
        }

        private void LoadData()
        {
            LoadExistingAccruals();
        }

        private void LoadExistingAccruals()
        {
            ExistingAccruals.Clear();
            var list = _accrualRepository.GetByPeriod(_selectedYear, _selectedMonth);
            foreach (var item in list)
                ExistingAccruals.Add(item);
        }
    }
}
