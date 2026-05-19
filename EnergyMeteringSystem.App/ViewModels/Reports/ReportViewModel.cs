using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.Views.Shared;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services;
using EnergyMeteringSystem.Services.Export;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly ReportRepository _reportRepository;
        private readonly ExportService _exportService;

        private int _selectedYear;
        private int _selectedMonth;
        private string _selectedMonthName;
        private int _selectedReportType;
        private object _reportData;
        private string _dateError;
        private DateTime _startDate;
        private DateTime _endDate;

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
            SetReportTypeCommand = new RelayCommand<int>(type => SelectedReportType = type);

            InitializeData();
        }

        // ===== КОЛЛЕКЦИИ =====
        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<ConsumptionReportDto> ConsumptionData { get; set; }
        public ObservableCollection<AccrualReportDto> AccrualData { get; set; }
        public ObservableCollection<DebtDto> DebtData { get; set; }

        // ===== ВЫБРАННЫЙ ОТЧЁТ =====
        public int SelectedReportType
        {
            get => _selectedReportType;
            set
            {
                if (SetProperty(ref _selectedReportType, value))
                {
                    OnPropertyChanged(nameof(IsConsumptionReport));
                    OnPropertyChanged(nameof(IsAccrualReport));
                    OnPropertyChanged(nameof(IsDebtReport));
                    OnPropertyChanged(nameof(ConsumptionVisibility));
                    OnPropertyChanged(nameof(AccrualVisibility));
                    OnPropertyChanged(nameof(DebtVisibility));
                    LoadReport();
                }
            }
        }

        // ===== ДЛЯ RADIOBUTTON =====
        public bool IsConsumptionReport
        {
            get => _selectedReportType == 0;
            set { if (value) SelectedReportType = 0; }
        }
        public bool IsAccrualReport
        {
            get => _selectedReportType == 1;
            set { if (value) SelectedReportType = 1; }
        }
        public bool IsDebtReport
        {
            get => _selectedReportType == 2;
            set { if (value) SelectedReportType = 2; }
        }

        // ===== ВИДИМОСТЬ ПАНЕЛЕЙ =====
        public Visibility ConsumptionVisibility => IsConsumptionReport ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AccrualVisibility => IsAccrualReport ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DebtVisibility => IsDebtReport ? Visibility.Visible : Visibility.Collapsed;

        // ===== ДАННЫЕ ДЛЯ ТАБЛИЦЫ =====
        public object ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }

        // ===== ПАРАМЕТРЫ ОТЧЁТОВ =====
        public int SelectedYear
        {
            get => _selectedYear;
            set { SetProperty(ref _selectedYear, value); LoadReport(); }
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
                if (SetProperty(ref _startDate, value))
                {
                    ValidateDates();
                    LoadReport();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                {
                    ValidateDates();
                    LoadReport();
                }
            }
        }

        public string DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

        private void ValidateDates()
        {
            if (StartDate > EndDate)
            {
                DateError = "Дата начала не может быть позже даты окончания";
                ToastNotificationService.ShowNear(null, DateError, 2000);
            }
            else
            {
                DateError = string.Empty;
            }
        }

        // ===== ИТОГОВЫЕ СВОЙСТВА =====
        public decimal TotalConsumption => ConsumptionData?.Sum(c => c.Consumption) ?? 0;
        public decimal TotalAccrual => AccrualData?.Sum(a => a.AccrualAmount) ?? 0;
        public decimal TotalPaid => AccrualData?.Sum(a => a.PaidAmount) ?? 0;
        public decimal TotalDebtSum => AccrualData?.Sum(a => a.DebtAmount) ?? 0;
        public decimal TotalDebtAll => DebtData?.Sum(d => d.DebtAmount) ?? 0;
        public int DebtorsCount => DebtData?.Count ?? 0;
        public string TotalDebtColor => TotalDebtSum > 0 ? "Red" : "Black";

        // ===== КОМАНДЫ =====
        public RelayCommand<int> SetReportTypeCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportCommand { get; }

        // ===== ИНИЦИАЛИЗАЦИЯ =====
        private void InitializeData()
        {
            for (int i = 2020; i <= DateTime.Today.Year + 1; i++)
                Years.Add(i);

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

        // ===== ЗАГРУЗКА ДАННЫХ =====
        private void LoadReport()
        {
            try
            {
                switch (_selectedReportType)
                {
                    case 0:
                        var consumptionData = _reportRepository.GetConsumptionReport(_startDate, _endDate);
                        ConsumptionData.Clear();
                        foreach (var item in consumptionData)
                            ConsumptionData.Add(item);
                        ReportData = ConsumptionData;
                        break;
                    case 1:
                        var accrualData = _reportRepository.GetAccrualReport(_selectedYear, _selectedMonth);
                        AccrualData.Clear();
                        foreach (var item in accrualData)
                            AccrualData.Add(item);
                        ReportData = AccrualData;
                        break;
                    case 2:
                        var debtData = _reportRepository.GetDebtReport();
                        DebtData.Clear();
                        foreach (var item in debtData)
                            DebtData.Add(item);
                        ReportData = DebtData;
                        break;
                }

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

        // ===== ЭКСПОРТ =====
        private void ExportReport()
        {
            try
            {
                DataGrid tempGrid = null;

                switch (_selectedReportType)
                {
                    case 0: // Потребление
                        if (ConsumptionData.Any())
                        {
                            tempGrid = CreateTempDataGridForConsumption();
                        }
                        break;
                    case 1: // Начисления
                        if (AccrualData.Any())
                        {
                            tempGrid = CreateTempDataGridForAccrual();
                        }
                        break;
                    case 2: // Задолженность
                        if (DebtData.Any())
                        {
                            tempGrid = CreateTempDataGridForDebt();
                        }
                        break;
                }

                if (tempGrid != null)
                {
                    string fileName = GetFileName();
                    _exportService.ExportWithFormatDialog(tempGrid, fileName);
                }
                else
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetFileName()
        {
            switch (_selectedReportType)
            {
                case 0:
                    return $"Отчет_потребление_{StartDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}";
                case 1:
                    return $"Отчет_начисления_{SelectedMonthName}_{SelectedYear}";
                case 2:
                    return $"Отчет_задолженность_{DateTime.Now:dd.MM.yyyy}";
                default:
                    return "Отчет";
            }
        }

        private DataGrid CreateTempDataGridForConsumption()
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = ConsumptionData,
                IsReadOnly = true
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Адрес", Binding = new System.Windows.Data.Binding("Address") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Счетчик", Binding = new System.Windows.Data.Binding("MeterSerial") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Период", Binding = new System.Windows.Data.Binding("PeriodText") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Нач. показание", Binding = new System.Windows.Data.Binding("StartValue") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Кон. показание", Binding = new System.Windows.Data.Binding("EndValue") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Потребление (кВт)", Binding = new System.Windows.Data.Binding("Consumption") });

            return grid;
        }

        private DataGrid CreateTempDataGridForAccrual()
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = AccrualData,
                IsReadOnly = true
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Адрес", Binding = new System.Windows.Data.Binding("Address") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Период", Binding = new System.Windows.Data.Binding("PeriodText") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Начислено", Binding = new System.Windows.Data.Binding("AccrualText") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Оплачено", Binding = new System.Windows.Data.Binding("PaidText") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Долг", Binding = new System.Windows.Data.Binding("DebtText") });

            return grid;
        }

        private DataGrid CreateTempDataGridForDebt()
        {
            var grid = new DataGrid

            {
                AutoGenerateColumns = false,
                ItemsSource = DebtData,
                IsReadOnly = true
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Адрес", Binding = new System.Windows.Data.Binding("Address") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Сумма долга", Binding = new System.Windows.Data.Binding("DebtAmountText") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Период", Binding = new System.Windows.Data.Binding("PeriodText") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Просрочка", Binding = new System.Windows.Data.Binding("OverdueText") });

            return grid;
        }
    }
}