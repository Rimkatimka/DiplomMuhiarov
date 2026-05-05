using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Export;

namespace EnergyMeteringSystem.App.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly ReportRepository _reportRepository;
        private readonly ExportService _exportService;

        private int _selectedYear;
        private int _selectedMonth;
        private string _selectedMonthName;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _selectedReportType;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<ConsumptionReportDto> ConsumptionData { get; set; }
        public ObservableCollection<AccrualReportDto> AccrualData { get; set; }
        public ObservableCollection<DebtDto> DebtData { get; set; }

        // ===== СВОЙСТВА ДЛЯ ВИДИМОСТИ =====
        public bool IsConsumptionReport => _selectedReportType == 0;
        public bool IsAccrualReport => _selectedReportType == 1;
        public bool IsDebtReport => _selectedReportType == 2;

        // ===== СВОЙСТВА ДЛЯ ВЫБОРА =====
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                SetProperty(ref _selectedYear, value);
                LoadReport();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                SetProperty(ref _selectedMonth, value);
                if (Months != null && _selectedMonth > 0 && _selectedMonth <= Months.Count)
                    SelectedMonthName = Months[_selectedMonth - 1];
                LoadReport();
            }
        }

        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                SetProperty(ref _selectedMonthName, value);
                if (Months != null && !string.IsNullOrEmpty(value))
                {
                    int monthIndex = Months.IndexOf(value) + 1;
                    if (monthIndex > 0 && monthIndex != _selectedMonth)
                    {
                        _selectedMonth = monthIndex;
                        LoadReport();
                    }
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                LoadReport();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value);
                LoadReport();
            }
        }

        public int SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                SetProperty(ref _selectedReportType, value);
                // Уведомляем об изменении свойств видимости
                OnPropertyChanged(nameof(IsConsumptionReport));
                OnPropertyChanged(nameof(IsAccrualReport));
                OnPropertyChanged(nameof(IsDebtReport));
                LoadReport();
            }
        }

        // ===== ИТОГОВЫЕ СВОЙСТВА =====
        public decimal TotalConsumption => ConsumptionData?.Sum(c => c.Consumption) ?? 0;
        public decimal TotalAccrual => AccrualData?.Sum(a => a.AccrualAmount) ?? 0;
        public decimal TotalPaid => AccrualData?.Sum(a => a.PaidAmount) ?? 0;
        public decimal TotalDebtSum => AccrualData?.Sum(a => a.DebtAmount) ?? 0;
        public decimal TotalDebtAll => DebtData?.Sum(d => d.DebtAmount) ?? 0;
        public int DebtorsCount => DebtData?.Count ?? 0;

        // ===== ЦВЕТ ДЛЯ ДОЛГА (без конвертера) =====
        public string TotalDebtColor => TotalDebtSum > 0 ? "Red" : "Black";

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportCommand { get; }

        public ReportViewModel()
        {
            _reportRepository = new ReportRepository();
            _exportService = new ExportService();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            ConsumptionData = new ObservableCollection<ConsumptionReportDto>();
            AccrualData = new ObservableCollection<AccrualReportDto>();
            DebtData = new ObservableCollection<DebtDto>();

            RefreshCommand = new RelayCommand(_ => LoadReport());
            ExportCommand = new RelayCommand(_ => ExportReport());

            InitializeData();
        }

        private void InitializeData()
        {
            // Годы
            for (int i = 2020; i <= DateTime.Today.Year + 1; i++)
                Years.Add(i);

            // Месяцы
            Months.Add("Январь"); Months.Add("Февраль"); Months.Add("Март");
            Months.Add("Апрель"); Months.Add("Май"); Months.Add("Июнь");
            Months.Add("Июль"); Months.Add("Август"); Months.Add("Сентябрь");
            Months.Add("Октябрь"); Months.Add("Ноябрь"); Months.Add("Декабрь");

            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;
            _selectedMonthName = Months[_selectedMonth - 1];
            _startDate = DateTime.Today.AddMonths(-3);
            _endDate = DateTime.Today;
            _selectedReportType = 0;

            LoadReport();
        }

        private void LoadReport()
        {
            try
            {
                ConsumptionData.Clear();
                AccrualData.Clear();
                DebtData.Clear();

                switch (_selectedReportType)
                {
                    case 0:
                        var consumption = _reportRepository.GetConsumptionReport(_startDate, _endDate);
                        System.Diagnostics.Debug.WriteLine($"Loaded {consumption.Count} consumption records");
                        foreach (var item in consumption)
                            ConsumptionData.Add(item);
                        break;

                    case 1:
                        var accrual = _reportRepository.GetAccrualReport(_selectedYear, _selectedMonth);
                        System.Diagnostics.Debug.WriteLine($"Loaded {accrual.Count} accrual records");
                        foreach (var item in accrual)
                            AccrualData.Add(item);
                        break;

                    case 2:
                        var debt = _reportRepository.GetDebtReport();
                        System.Diagnostics.Debug.WriteLine($"Loaded {debt.Count} debt records");
                        foreach (var item in debt)
                            DebtData.Add(item);
                        break;
                }

                // Уведомляем об изменении итоговых свойств
                OnPropertyChanged(nameof(TotalConsumption));
                OnPropertyChanged(nameof(TotalAccrual));
                OnPropertyChanged(nameof(TotalPaid));
                OnPropertyChanged(nameof(TotalDebtSum));
                OnPropertyChanged(nameof(TotalDebtAll));
                OnPropertyChanged(nameof(DebtorsCount));
                OnPropertyChanged(nameof(TotalDebtColor));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading report: {ex.Message}");
                MessageBox.Show($"Ошибка при загрузке отчета: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReport()
        {
            try
            {
                switch (_selectedReportType)
                {
                    case 0:
                        if (ConsumptionData.Any())
                            _exportService.ExportConsumptionReport(ConsumptionData.ToList(), _startDate, _endDate);
                        else
                            MessageBox.Show("Нет данных для экспорта", "Информация",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    case 1:
                        if (AccrualData.Any())
                            _exportService.ExportAccrualReport(AccrualData.ToList(), _selectedYear, _selectedMonth);
                        else
                            MessageBox.Show("Нет данных для экспорта", "Информация",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        break;

                    case 2:
                        if (DebtData.Any())
                            _exportService.ExportDebtReport(DebtData.ToList());
                        else
                            MessageBox.Show("Нет данных для экспорта", "Информация",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}