using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.Views.Shared;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Calculation;
using EnergyMeteringSystem.Services.Export;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Billing
{
    public class AccrualViewModel : ViewModelBase
    {
        private readonly AccrualRepository _accrualRepository;
        private readonly CalculationService _calculationService;

        private int _selectedYear;
        private int _selectedMonth;
        private AccrualCalculationDto _selectedItem;
        private string _periodWarning;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<AccrualCalculationDto> Calculations { get; set; }
        public ObservableCollection<AccrualDto> ExistingAccruals { get; set; }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _ = SetProperty(ref _selectedYear, value);
                CheckPeriodAvailability();
                LoadData();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _ = SetProperty(ref _selectedMonth, value);
                CheckPeriodAvailability();
                LoadData();
            }
        }

        public string PeriodWarning
        {
            get => _periodWarning;
            set => SetProperty(ref _periodWarning, value);
        }

        public bool HasPeriodWarning => !string.IsNullOrEmpty(PeriodWarning);

        private string _selectedMonthName;
        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                if (SetProperty(ref _selectedMonthName, value))
                {
                    int monthIndex = Months.IndexOf(value) + 1;
                    if (monthIndex != _selectedMonth)
                    {
                        _selectedMonth = monthIndex;
                        LoadData();
                    }
                }
            }
        }

        public AccrualCalculationDto SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanSave()
        {
            return SelectedItem != null &&
                   !SelectedItem.HasExistingAccrual &&
                   CanCalculateForPeriod();  // ← нельзя сохранять для будущих периодов
        }

        public decimal TotalAmount => Calculations?.Sum(c => c.TotalAmount) ?? 0;

        public RelayCommand CalculateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportCommand { get; }

        public AccrualViewModel()
        {
            _accrualRepository = new AccrualRepository();
            _calculationService = new CalculationService();

            CalculateCommand = new RelayCommand(_ => Calculate(), _ => CanCalculate());
            SaveCommand = new RelayCommand(_ => SaveSelected(), _ => CanSave());
            RefreshCommand = new RelayCommand(_ => LoadData());
            ExportCommand = new RelayCommand(_ => ExportToExcel());

            Years = [];
            Months = [];
            Calculations = [];
            ExistingAccruals = [];

            InitializeYearsAndMonths();
        }

        private void InitializeYearsAndMonths()
        {
            // Годы: от 2020 до текущего + 1 (для просмотра, но не для расчёта)
            for (int i = 2020; i <= DateTime.Today.Year + 1; i++)
                Years.Add(i);

            Months.Clear();
            Months.Add("Январь"); Months.Add("Февраль"); Months.Add("Март");
            Months.Add("Апрель"); Months.Add("Май"); Months.Add("Июнь");
            Months.Add("Июль"); Months.Add("Август"); Months.Add("Сентябрь");
            Months.Add("Октябрь"); Months.Add("Ноябрь"); Months.Add("Декабрь");

            // По умолчанию выбираем предыдущий месяц (за который можно делать расчёт)
            var defaultPeriod = GetDefaultCalculationPeriod();
            _selectedYear = defaultPeriod.Year;
            _selectedMonth = defaultPeriod.Month;
            _selectedMonthName = Months[_selectedMonth - 1];

            CheckPeriodAvailability();
        }

        /// <summary>
        /// Определяет, можно ли делать начисление за выбранный период
        /// По закону: начисление делается только за ПРОШЕДШИЕ месяцы
        /// </summary>
        private bool CanCalculateForPeriod()
        {
            var selectedDate = new DateTime(_selectedYear, _selectedMonth, 1);
            var today = DateTime.Today;

            // Нельзя начислять за будущий месяц
            if (selectedDate > today)
            {
                PeriodWarning = $"Нельзя начислять за будущий период ({SelectedMonthName} {_selectedYear})";
                return false;
            }

            // Нельзя начислять за текущий месяц, пока он не закончился
            if (selectedDate.Year == today.Year && selectedDate.Month == today.Month)
            {
                PeriodWarning = $"Начисление за {SelectedMonthName} {_selectedYear} будет доступно с 1 числа следующего месяца";
                return false;
            }

            PeriodWarning = string.Empty;
            return true;
        }

        /// <summary>
        /// Возвращает период по умолчанию для расчёта (предыдущий месяц)
        /// </summary>
        private (int Year, int Month) GetDefaultCalculationPeriod()
        {
            var today = DateTime.Today;
            var previousMonth = today.AddMonths(-1);
            return (previousMonth.Year, previousMonth.Month);
        }

        private void CheckPeriodAvailability()
        {
            CanCalculateForPeriod();
            // Обновляем состояние кнопки "Рассчитать"
            CalculateCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool CanCalculate()
        {
            return _selectedYear > 0 &&
                   _selectedMonth > 0 &&
                   CanCalculateForPeriod();
        }

        private void Calculate()
        {
            if (!CanCalculateForPeriod())
            {
                MessageBox.Show(PeriodWarning, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var results = _calculationService.CalculateForPeriod(_selectedYear, _selectedMonth);

            System.Diagnostics.Debug.WriteLine($"CalculateForPeriod({_selectedYear}, {_selectedMonth}) -> {results.Count} записей");

            Calculations.Clear();
            foreach (var item in results)
                Calculations.Add(item);

            OnPropertyChanged(nameof(TotalAmount));
        }

        private void SaveSelected()
        {
            if (SelectedItem == null)
            {
                MessageBox.Show("Выберите объект для сохранения", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!CanCalculateForPeriod())
            {
                MessageBox.Show(PeriodWarning, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AccrualDto dto = new()
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

            MessageBox.Show($"Начисление для {SelectedItem.Address} сохранено", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadData()
        {
            LoadExistingAccruals();
        }

        private void LoadExistingAccruals()
        {
            ExistingAccruals.Clear();
            var list = _accrualRepository.GetByPeriod(_selectedYear, _selectedMonth);

            System.Diagnostics.Debug.WriteLine($"GetByPeriod({_selectedYear}, {_selectedMonth}) -> {list.Count} записей");

            foreach (var item in list)
                ExistingAccruals.Add(item);
        }

        private void ExportToExcel()
        {
            var dialog = new ExportFormatDialog();
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                var exportService = new ExportService();
                MessageBox.Show($"Выбран формат: {dialog.SelectedFormat}", "Экспорт");
            }
        }
    }
}