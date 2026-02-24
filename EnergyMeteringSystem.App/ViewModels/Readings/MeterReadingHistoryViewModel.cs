using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingHistoryViewModel : ViewModelBase
    {
        public LiveCharts.SeriesCollection SeriesCollection { get; set; }
        public string[] ChartDates { get; set; }
        public Func<double, string> YFormatter { get; set; }

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

        // Для графика
        public ObservableCollection<ChartPoint> ChartData { get; set; }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                LoadMeters();
                LoadHistory();
            }
        }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                SetProperty(ref _selectedMeter, value);
                LoadHistory();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                LoadHistory();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value);
                LoadHistory();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ExportCommand { get; }

        public MeterReadingHistoryViewModel()
        {
            _objectRepository = new ConsumptionObjectRepository();
            _meterRepository = new MeterRepository();
            _readingRepository = new MeterReadingRepository();

            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Meters = new ObservableCollection<MeterDto>();
            Readings = new ObservableCollection<MeterReadingHistoryDto>();
            ChartData = new ObservableCollection<ChartPoint>();

            _startDate = DateTime.Today.AddMonths(-6);
            _endDate = DateTime.Today;

            RefreshCommand = new RelayCommand(_ => LoadData());
            ExportCommand = new RelayCommand(_ => ExportToExcel());

            SeriesCollection = new LiveCharts.SeriesCollection();
            YFormatter = value => value.ToString("N0");

            LoadObjects();
        }

        private void LoadObjects()
        {
            Objects.Clear();
            var list = _objectRepository.GetAll();
            foreach (var obj in list)
                Objects.Add(obj);
        }

        private void LoadMeters()
        {
            Meters.Clear();
            if (_selectedObject == null) return;

            var list = _meterRepository.GetByObjectId(_selectedObject.Id);
            foreach (var meter in list)
                Meters.Add(meter);
        }

        private void LoadHistory()
        {
            Readings.Clear();
            SeriesCollection.Clear();

            if (_selectedMeter == null) return;

            var history = _readingRepository.GetHistoryByMeterId(_selectedMeter.Id);

            // Фильтр по датам
            var filtered = history.Where(h =>
                h.ReadingDate >= _startDate &&
                h.ReadingDate <= _endDate)
                .OrderBy(h => h.ReadingDate)
                .ToList();

            foreach (var item in filtered)
                Readings.Add(item);

            if (filtered.Any())
            {
                // Данные для графика
                var values = filtered.Select(h => (double)h.Value).ToArray();
                var consumptions = filtered.Select(h => (double)(h.Consumption ?? 0)).ToArray();

                ChartDates = filtered.Select(h => h.ReadingDate.ToString("dd.MM")).ToArray();

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

        private void LoadData()
        {
            LoadObjects();
            if (Objects.Any())
                SelectedObject = Objects.First();
        }

        private void ExportToExcel()
        {
            // TODO: реализовать экспорт
        }
    }

    // Класс для точек графика
    public class ChartPoint
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public decimal Consumption { get; set; }
    }
}
