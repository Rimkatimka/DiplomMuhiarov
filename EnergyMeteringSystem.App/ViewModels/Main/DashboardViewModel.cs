using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
            ChartYears = new ObservableCollection<int>();

            RefreshCommand = new AsyncRelayCommand(async () => await LoadDataAsync());

            for (int i = 2020; i <= DateTime.Today.Year; i++)
                ChartYears.Add(i);

            _selectedChartYear = DateTime.Today.Year;

            _ = LoadDataAsync();
            _ = LoadChartDataAsync();
        }

        public int TotalObjects => _data?.TotalObjects ?? 0;
        public int TotalMeters => _data?.TotalMeters ?? 0;
        public int ReadingsToday => _data?.ReadingsToday ?? 0;
        public int ReadingsWeek => _data?.ReadingsWeek ?? 0;
        public int ExpiredMeters => _data?.ExpiredMeters ?? 0;

        public ObservableCollection<int> ChartYears { get; set; }

        public int SelectedChartYear
        {
            get => _selectedChartYear;
            set
            {
                if (SetProperty(ref _selectedChartYear, value))
                {
                    _ = LoadChartDataAsync();
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

        public AsyncRelayCommand RefreshCommand { get; }

        private async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                _data = await _dashboardRepository.GetDashboardDataAsync();

                OnPropertyChanged(nameof(TotalObjects));
                OnPropertyChanged(nameof(TotalMeters));
                OnPropertyChanged(nameof(ReadingsToday));
                OnPropertyChanged(nameof(ReadingsWeek));
                OnPropertyChanged(nameof(ExpiredMeters));
            }, "Ошибка загрузки дашборда");
        }

        private async Task LoadChartDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var data = await _dashboardRepository.GetChartDataAsync(_selectedChartYear);

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
            }, "Ошибка загрузки графика");
        }
    }
}