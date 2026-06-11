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

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingHistoryViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterRepository _meterRepository;
        private readonly MeterReadingRepository _readingRepository;

        private ConsumptionObjectDto _selectedObject;
        private MeterDto _selectedMeter;
        private DateTime _startDate;
        private DateTime _endDate;

        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterDto> Meters { get; set; }
        public ObservableCollection<MeterReadingHistoryDto> Readings { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] ChartDates { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (SetProperty(ref _selectedObject, value))
                {
                    _ = LoadMetersAsync();
                    _ = LoadHistoryAsync();
                }
            }
        }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                if (SetProperty(ref _selectedMeter, value))
                    _ = LoadHistoryAsync();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    _ = LoadHistoryAsync();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    _ = LoadHistoryAsync();
            }
        }

        public bool HasData => Readings != null && Readings.Count > 0;

        public AsyncRelayCommand RefreshCommand { get; }

        public MeterReadingHistoryViewModel()
        {
            _objectRepository = new ConsumptionObjectRepository();
            _meterRepository = new MeterRepository();
            _readingRepository = new MeterReadingRepository();

            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Meters = new ObservableCollection<MeterDto>();
            Readings = new ObservableCollection<MeterReadingHistoryDto>();

            SeriesCollection = new SeriesCollection();
            YFormatter = value => value.ToString("N0");

            _startDate = DateTime.Today.AddMonths(-6);
            _endDate = DateTime.Today;

            RefreshCommand = new AsyncRelayCommand(async () => await LoadHistoryAsync());

            _ = LoadObjectsAsync();
        }

        private async Task LoadObjectsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var list = await _objectRepository.GetAllAsync();
                Objects.Clear();
                foreach (var obj in list)
                    Objects.Add(obj);
            }, "Ошибка загрузки объектов");
        }

        private async Task LoadMetersAsync()
        {
            await ExecuteAsync(async () =>
            {
                Meters.Clear();
                if (_selectedObject == null) return;

                var list = await _meterRepository.GetByObjectIdAsync(_selectedObject.Id);
                foreach (var meter in list)
                    Meters.Add(meter);
            }, "Ошибка загрузки счетчиков");
        }

        private async Task LoadHistoryAsync()
        {
            await ExecuteAsync(async () =>
            {
                Readings.Clear();
                SeriesCollection.Clear();

                if (_selectedMeter == null) return;

                var history = await _readingRepository.GetHistoryByMeterIdAsync(_selectedMeter.Id);

                var filtered = history
                    .Where(h => h.ReadingDate >= _startDate && h.ReadingDate <= _endDate)
                    .OrderBy(h => h.ReadingDate)
                    .ToList();

                foreach (var item in filtered)
                    Readings.Add(item);

                UpdateChart(filtered);
            }, "Ошибка загрузки истории");
        }

        private void UpdateChart(System.Collections.Generic.List<MeterReadingHistoryDto> data)
        {
            SeriesCollection.Clear();

            if (!data.Any()) return;

            var values = data.Select(h => (double)h.Value).ToArray();
            var consumptions = data.Select(h => (double)(h.Consumption ?? 0)).ToArray();

            ChartDates = data.Select(h => h.ReadingDate.ToString("dd.MM")).ToArray();

            SeriesCollection.Add(new LineSeries
            {
                Title = "Показания",
                Values = new ChartValues<double>(values),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 5
            });

            SeriesCollection.Add(new ColumnSeries
            {
                Title = "Потребление",
                Values = new ChartValues<double>(consumptions),
                Fill = System.Windows.Media.Brushes.Orange
            });
        }
    }
}