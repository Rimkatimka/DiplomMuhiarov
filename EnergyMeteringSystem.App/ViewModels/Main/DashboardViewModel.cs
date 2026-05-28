using System;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using LiveCharts;
using LiveCharts.Wpf;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardRepository _dashboardRepository;
        private DashboardDto _data;
        private int _selectedChartYear;
        private SeriesCollection _chartSeries;
        private string[] _chartMonths;

        public DashboardViewModel()
        {
            _dashboardRepository = new DashboardRepository();
            TopDebtors = new ObservableCollection<DebtDto>();
            ChartYears = new ObservableCollection<int>();

            RefreshCommand = new RelayCommand(_ => LoadData());

            for (int i = 2020; i <= DateTime.Today.Year; i++)
                ChartYears.Add(i);

            _selectedChartYear = DateTime.Today.Year;

            LoadData();
            LoadChartData();
        }

        public int TotalObjects => _data?.TotalObjects ?? 0;
        public int TotalMeters => _data?.TotalMeters ?? 0;
        public int ReadingsToday => _data?.ReadingsToday ?? 0;
        public int ReadingsWeek => _data?.ReadingsWeek ?? 0;
        public string AccrualMonth => (_data?.AccrualMonth ?? 0).ToString("F0") + " ₽";
        public string PaymentMonth => (_data?.PaymentMonth ?? 0).ToString("F0") + " ₽";
        public int ExpiredMeters => _data?.ExpiredMeters ?? 0;

        public ObservableCollection<DebtDto> TopDebtors { get; set; }
        public ObservableCollection<int> ChartYears { get; set; }

        public int SelectedChartYear
        {
            get => _selectedChartYear;
            set
            {
                if (SetProperty(ref _selectedChartYear, value))
                {
                    LoadChartData();
                }
            }
        }

        public SeriesCollection ChartSeries
        {
            get => _chartSeries;
            set => SetProperty(ref _chartSeries, value);
        }

        public string[] ChartMonths
        {
            get => _chartMonths;
            set => SetProperty(ref _chartMonths, value);
        }

        public Func<double, string> YFormatter => value => $"{value:F0}";

        public RelayCommand RefreshCommand { get; }

        private void LoadData()
        {
            _data = _dashboardRepository.GetDashboardData();

            OnPropertyChanged(nameof(TotalObjects));
            OnPropertyChanged(nameof(TotalMeters));
            OnPropertyChanged(nameof(ReadingsToday));
            OnPropertyChanged(nameof(ReadingsWeek));
            OnPropertyChanged(nameof(AccrualMonth));
            OnPropertyChanged(nameof(PaymentMonth));
            OnPropertyChanged(nameof(ExpiredMeters));

            TopDebtors.Clear();
            if (_data?.TopDebtors != null)
            {
                foreach (DebtDto debtor in _data.TopDebtors)
                {
                    TopDebtors.Add(debtor);
                }
            }
        }

        private void LoadChartData()
        {
            var data = _dashboardRepository.GetChartData(_selectedChartYear);

            ChartMonths = data.Select(d => d.MonthName).ToArray();

            ChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Потребление",
                    Values = new ChartValues<decimal>(data.Select(d => d.Consumption)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.5
                }
            };
        }
    }
}