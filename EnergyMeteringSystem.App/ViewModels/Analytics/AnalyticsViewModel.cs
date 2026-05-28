using System;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using LiveCharts;
using LiveCharts.Wpf;

namespace EnergyMeteringSystem.App.ViewModels.Analytics
{
    public class AnalyticsViewModel : ViewModelBase
    {
        private readonly AnalyticsRepository _repository;

        private int _selectedYear;
        private string _selectedMonthName;
        private ObservableCollection<TopObjectDto> _topObjects;

        private SeriesCollection _topObjectsSeries;
        private string[] _topObjectsLabels;
        private SeriesCollection _typeDistributionSeries;

        private decimal _totalConsumption;
        private decimal _averageConsumption;
        private decimal _maxConsumption;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                    LoadData();
            }
        }

        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                if (SetProperty(ref _selectedMonthName, value))
                {
                    int monthIndex = Months.IndexOf(value) + 1;
                    if (monthIndex > 0)
                        LoadData();
                }
            }
        }

        public ObservableCollection<TopObjectDto> TopObjects { get; set; }

        public SeriesCollection TopObjectsSeries
        {
            get => _topObjectsSeries;
            set => SetProperty(ref _topObjectsSeries, value);
        }

        public string[] TopObjectsLabels
        {
            get => _topObjectsLabels;
            set => SetProperty(ref _topObjectsLabels, value);
        }

        public SeriesCollection TypeDistributionSeries
        {
            get => _typeDistributionSeries;
            set => SetProperty(ref _typeDistributionSeries, value);
        }

        public decimal TotalConsumption
        {
            get => _totalConsumption;
            set => SetProperty(ref _totalConsumption, value);
        }

        public decimal AverageConsumption
        {
            get => _averageConsumption;
            set => SetProperty(ref _averageConsumption, value);
        }

        public decimal MaxConsumption
        {
            get => _maxConsumption;
            set => SetProperty(ref _maxConsumption, value);
        }

        public Func<double, string> XFormatter => value => $"{value:F0}";

        public RelayCommand RefreshCommand { get; }

        public AnalyticsViewModel()
        {
            _repository = new AnalyticsRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            TopObjects = new ObservableCollection<TopObjectDto>();

            for (int i = 2020; i <= DateTime.Today.Year; i++)
                Years.Add(i);

            string[] monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                    "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            foreach (var m in monthNames)
                Months.Add(m);

            _selectedYear = DateTime.Today.Year;
            _selectedMonthName = Months[DateTime.Today.Month - 1];

            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        private void LoadData()
        {
            int month = Months.IndexOf(SelectedMonthName) + 1;
            AnalyticsDataDto data = _repository.GetConsumptionData(_selectedYear, month);

            TopObjects.Clear();
            foreach (TopObjectDto item in data.TopObjects)
                TopObjects.Add(item);

            TotalConsumption = data.TotalConsumption;
            AverageConsumption = data.AverageConsumption;
            MaxConsumption = data.MaxConsumption;

            var top10 = data.TopObjects.Take(10).ToList();
            TopObjectsLabels = top10.Select(o => o.Address.Length > 20
                ? o.Address.Substring(0, 20) + "..."
                : o.Address).ToArray();

            TopObjectsSeries = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "Потребление",
                    Values = new ChartValues<decimal>(top10.Select(o => o.Consumption)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:F0} кВт·ч"
                }
            };

            TypeDistributionSeries = new SeriesCollection();
            foreach (TypeDistributionDto type in data.TypeDistribution)
            {
                TypeDistributionSeries.Add(new PieSeries
                {
                    Title = type.TypeName,
                    Values = new ChartValues<decimal> { type.Consumption },
                    DataLabels = true,
                    LabelPoint = point => $"{type.TypeName}: {point.Y:F0} кВт·ч"
                });
            }
        }
    }
}