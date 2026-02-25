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

namespace EnergyMeteringSystem.App.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        private readonly ReportRepository _reportRepository;

        private int _selectedYear;
        private int _selectedMonth;
        private DateTime _startDate;
        private DateTime _endDate;
        private int _selectedReportType;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<ConsumptionReportDto> ConsumptionData { get; set; }
        public ObservableCollection<AccrualReportDto> AccrualData { get; set; }
        public ObservableCollection<DebtDto> DebtData { get; set; }

        // Простые свойства для видимости в XAML
        public bool ShowConsumption => _selectedReportType == 0;
        public bool ShowAccrual => _selectedReportType == 1;
        public bool ShowDebt => _selectedReportType == 2;

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
                LoadReport();
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
                // Обновляем видимость
                OnPropertyChanged(nameof(ShowConsumption));
                OnPropertyChanged(nameof(ShowAccrual));
                OnPropertyChanged(nameof(ShowDebt));
                LoadReport();
            }
        }

        public decimal TotalConsumption => ConsumptionData?.Sum(c => c.Consumption) ?? 0;
        public decimal TotalAccrual => AccrualData?.Sum(a => a.AccrualAmount) ?? 0;
        public decimal TotalPaid => AccrualData?.Sum(a => a.PaidAmount) ?? 0;
        public decimal TotalDebtSum => AccrualData?.Sum(a => a.DebtAmount) ?? 0;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportCommand { get; }

        public ReportViewModel()
        {
            _reportRepository = new ReportRepository();

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
            for (int i = 2020; i <= DateTime.Today.Year; i++)
                Years.Add(i);

            Months.Add("Январь"); Months.Add("Февраль"); Months.Add("Март");
            Months.Add("Апрель"); Months.Add("Май"); Months.Add("Июнь");
            Months.Add("Июль"); Months.Add("Август"); Months.Add("Сентябрь");
            Months.Add("Октябрь"); Months.Add("Ноябрь"); Months.Add("Декабрь");

            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;
            _startDate = DateTime.Today.AddMonths(-1);
            _endDate = DateTime.Today;
            _selectedReportType = 0;

            LoadReport();
        }

        private void LoadReport()
        {
            ConsumptionData.Clear();
            AccrualData.Clear();
            DebtData.Clear();

            switch (_selectedReportType)
            {
                case 0:
                    var consumption = _reportRepository.GetConsumptionReport(_startDate, _endDate);
                    foreach (var item in consumption)
                        ConsumptionData.Add(item);
                    break;

                case 1:
                    var accrual = _reportRepository.GetAccrualReport(_selectedYear, _selectedMonth);
                    foreach (var item in accrual)
                        AccrualData.Add(item);
                    break;

                case 2:
                    var debt = _reportRepository.GetDebtReport();
                    foreach (var item in debt)
                        DebtData.Add(item);
                    break;
            }

            OnPropertyChanged(nameof(TotalConsumption));
            OnPropertyChanged(nameof(TotalAccrual));
            OnPropertyChanged(nameof(TotalPaid));
            OnPropertyChanged(nameof(TotalDebtSum));
        }

        private void ExportReport()
        {
            System.Windows.MessageBox.Show(
                "Экспорт в Excel будет доступен в следующей версии",
                "Экспорт",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }
}
