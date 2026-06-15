using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardRepository _dashboardRepository;
        private DashboardDto _data;
        private SeriesCollection _chartSeries;
        private string[] _chartMonths;
        private bool _hasData;
        private string _chartTitle;
        private int _selectedYear;

        private int _totalObjects;
        private int _totalMeters;
        private int _readingsToday;
        private int _readingsWeek;
        private int _expiredMeters;

        private decimal _monthlyConsumption;
        private decimal _averagePerObject;

        public decimal MonthlyConsumption
        {
            get => _monthlyConsumption;
            set => SetProperty(ref _monthlyConsumption, value);
        }

        public decimal AveragePerObject
        {
            get => _averagePerObject;
            set => SetProperty(ref _averagePerObject, value);
        }
        // Коллекция годов (с 2024 по текущий)
        public ObservableCollection<int> AvailableYears { get; set; }

        public int TotalObjects
        {
            get => _totalObjects;
            set => SetProperty(ref _totalObjects, value);
        }

        public int TotalMeters
        {
            get => _totalMeters;
            set => SetProperty(ref _totalMeters, value);
        }

        public int ReadingsToday
        {
            get => _readingsToday;
            set => SetProperty(ref _readingsToday, value);
        }

        public int ReadingsWeek
        {
            get => _readingsWeek;
            set => SetProperty(ref _readingsWeek, value);
        }

        public int ExpiredMeters
        {
            get => _expiredMeters;
            set => SetProperty(ref _expiredMeters, value);
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

        public string ChartTitle
        {
            get => _chartTitle;
            set => SetProperty(ref _chartTitle, value);
        }

        public bool HasData
        {
            get => _hasData;
            set => SetProperty(ref _hasData, value);
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    _ = LoadChartDataAsync();
                }
            }
        }

        public Func<double, string> YFormatter => value => $"{value:F0}";

        public AsyncRelayCommand RefreshCommand { get; }

        public DashboardViewModel()
        {
            System.Diagnostics.Debug.WriteLine("=== DashboardViewModel конструктор ===");

            _dashboardRepository = new DashboardRepository();
            ChartSeries = new SeriesCollection();
            ChartMonths = Array.Empty<string>();
            ChartTitle = "Загрузка данных...";
            HasData = true;

            // Инициализация списка годов (с 2024 по текущий)
            AvailableYears = new ObservableCollection<int>();
            for (int i = 2024; i <= DateTime.Today.Year; i++)
            {
                AvailableYears.Add(i);
            }
            _selectedYear = DateTime.Today.Year;

            RefreshCommand = new AsyncRelayCommand(async () => await LoadAllDataAsync());

            _ = LoadAllDataAsync();
        }

        private async Task LoadAllDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("LoadAllDataAsync START");

            try
            {
                // Загружаем KPI
                _data = await _dashboardRepository.GetDashboardDataAsync();

                System.Diagnostics.Debug.WriteLine($"GetDashboardDataAsync: Objects={_data?.TotalObjects}, Meters={_data?.TotalMeters}");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    TotalObjects = _data?.TotalObjects ?? 0;
                    TotalMeters = _data?.TotalMeters ?? 0;
                    ReadingsToday = _data?.ReadingsToday ?? 0;
                    ReadingsWeek = _data?.ReadingsWeek ?? 0;
                    ExpiredMeters = _data?.ExpiredMeters ?? 0;

                    System.Diagnostics.Debug.WriteLine($"KPI обновлены: Objects={TotalObjects}, Meters={TotalMeters}");
                });
                var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var monthlyData = await _dashboardRepository.GetMonthlyConsumptionAsync(DateTime.Today.Year);
                var currentMonthConsumption = monthlyData[DateTime.Today.Month - 1]?.Consumption ?? 0;
                MonthlyConsumption = currentMonthConsumption;
                AveragePerObject = TotalObjects > 0 ? MonthlyConsumption / TotalObjects : 0;
                // Загружаем данные для графика
                await LoadChartDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAllDataAsync ERROR: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ChartTitle = "Ошибка загрузки данных";
                    HasData = false;
                });
            }
        }

        private async Task LoadChartDataAsync()
        {
            System.Diagnostics.Debug.WriteLine($"LoadChartDataAsync START for year {SelectedYear}");

            try
            {
                // Получаем данные о потреблении по месяцам
                var chartData = await _dashboardRepository.GetMonthlyConsumptionAsync(SelectedYear);

                System.Diagnostics.Debug.WriteLine($"GetMonthlyConsumptionAsync вернул {chartData?.Count ?? 0} точек");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Проверяем есть ли ХОТЯ БЫ ОДНО значение > 0
                    bool hasAnyPositiveData = chartData != null && chartData.Any(d => d.Consumption > 0);

                    if (hasAnyPositiveData)
                    {
                        ChartMonths = chartData.Select(d => d.MonthName).ToArray();

                        ChartSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Потребление, кВт·ч",
                        Values = new ChartValues<decimal>(chartData.Select(d => d.Consumption)),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:F0}"
                    }
                };

                        ChartTitle = $"Динамика потребления по месяцам ({SelectedYear} год)";
                        HasData = true;

                        System.Diagnostics.Debug.WriteLine($"График обновлен, сумма потребления: {chartData.Sum(d => d.Consumption):F0} кВт·ч");
                    }
                    else
                    {
                        // Нет данных — показываем заглушку
                        ChartMonths = new[] { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
                        ChartSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Нет данных",
                        Values = new ChartValues<decimal>(new decimal[12]),
                        DataLabels = false
                    }
                };
                        ChartTitle = $"Нет данных за {SelectedYear} год";
                        HasData = false;
                    }

                    OnPropertyChanged(nameof(ChartSeries));
                    OnPropertyChanged(nameof(ChartMonths));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadChartDataAsync ERROR: {ex.Message}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ChartTitle = "Ошибка загрузки графика";
                    HasData = false;
                });
            }
        }
    }
}