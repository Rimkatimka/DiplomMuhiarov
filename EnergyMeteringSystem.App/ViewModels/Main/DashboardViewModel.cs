using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private SeriesCollection _chartSeries;
        private string[] _chartMonths;

        // KPI свойства
        private int _totalObjects;
        private int _totalMeters;
        private int _readingsToday;
        private int _readingsWeek;
        private int _expiredMeters;
        private bool _hasData;
        private string _chartTitle;

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

            RefreshCommand = new AsyncRelayCommand(async () => await LoadAllDataAsync());

            _ = LoadAllDataAsync();
        }

        private async Task LoadAllDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("LoadAllDataAsync START");

            await ExecuteAsync(async () =>
            {
                // Загружаем KPI
                _data = await _dashboardRepository.GetDashboardDataAsync();

                System.Diagnostics.Debug.WriteLine($"GetDashboardDataAsync: Objects={_data?.TotalObjects}, Meters={_data?.TotalMeters}, ReadingsToday={_data?.ReadingsToday}");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    TotalObjects = _data?.TotalObjects ?? 0;
                    TotalMeters = _data?.TotalMeters ?? 0;
                    ReadingsToday = _data?.ReadingsToday ?? 0;
                    ReadingsWeek = _data?.ReadingsWeek ?? 0;
                    ExpiredMeters = _data?.ExpiredMeters ?? 0;

                    System.Diagnostics.Debug.WriteLine($"KPI обновлены: Objects={TotalObjects}, Meters={TotalMeters}");
                });

                // Загружаем данные для графика (все года сразу или последний год)
                await LoadChartDataSimpleAsync();

            }, "Ошибка загрузки данных дашборда");
        }

        private async Task LoadChartDataSimpleAsync()
        {
            System.Diagnostics.Debug.WriteLine("LoadChartDataSimpleAsync START");

            await ExecuteAsync(async () =>
            {
                // Получаем все данные для графика
                var chartData = await _dashboardRepository.GetAllChartDataAsync();

                System.Diagnostics.Debug.WriteLine($"GetAllChartDataAsync вернул {chartData?.Count ?? 0} точек");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (chartData != null && chartData.Any(d => d.Consumption > 0))
                    {
                        ChartMonths = chartData.Select(d => d.MonthName).ToArray();

                        ChartSeries = new SeriesCollection
                        {
                            new ColumnSeries
                            {
                                Title = "Количество показаний",
                                Values = new ChartValues<decimal>(chartData.Select(d => d.Consumption)),
                                DataLabels = true,
                                LabelPoint = point => $"{point.Y:F0}"
                            }
                        };

                        ChartTitle = "Динамика показаний по месяцам";
                        HasData = true;

                        System.Diagnostics.Debug.WriteLine($"График обновлен, месяцев: {ChartMonths.Length}");
                        System.Diagnostics.Debug.WriteLine($"Данные: {string.Join(", ", chartData.Select(d => d.Consumption))}");
                    }
                    else
                    {
                        // Нет данных
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
                        ChartTitle = "Нет данных для отображения";
                        HasData = false;

                        System.Diagnostics.Debug.WriteLine("Нет данных для графика");
                    }
                });
            }, "Ошибка загрузки графика");
        }
    }
}