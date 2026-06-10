using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        // Коллекции для ComboBox
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterDto> Meters { get; set; }
        public ObservableCollection<MeterReadingHistoryDto> Readings { get; set; }

        // Для графика
        public SeriesCollection SeriesCollection { get; set; }
        public string[] ChartDates { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                _ = SetProperty(ref _selectedObject, value);
                LoadMeters(); // Загружаем счетчики при выборе объекта
                LoadHistory();
            }
        }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                _ = SetProperty(ref _selectedMeter, value);
                LoadHistory();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _ = SetProperty(ref _startDate, value);
                LoadHistory();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _ = SetProperty(ref _endDate, value);
                LoadHistory();
            }
        }

        public bool HasData => Readings != null && Readings.Any();

        public RelayCommand RefreshCommand { get; }

        public MeterReadingHistoryViewModel()
        {
            _objectRepository = new ConsumptionObjectRepository();
            _meterRepository = new MeterRepository();
            _readingRepository = new MeterReadingRepository();

            Objects = [];
            Meters = [];
            Readings = [];

            SeriesCollection = [];
            YFormatter = value => value.ToString("N0");

            _startDate = DateTime.Today.AddMonths(-6);
            _endDate = DateTime.Today;

            RefreshCommand = new RelayCommand(_ => LoadHistory());

            // Загружаем объекты при создании ViewModel
            LoadObjects();
        }

        private void LoadObjects()
        {
            try
            {
                Objects.Clear();
                List<ConsumptionObjectDto> list = _objectRepository.GetAll();
                System.Diagnostics.Debug.WriteLine($"LoadObjects: загружено {list.Count} объектов");

                foreach (ConsumptionObjectDto obj in list)
                {
                    Objects.Add(obj);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки объектов: {ex.Message}");
            }
        }

        private void LoadMeters()
        {
            try
            {
                Meters.Clear();
                if (_selectedObject == null)
                {
                    return;
                }

                List<MeterDto> list = _meterRepository.GetByObjectId(_selectedObject.Id);
                System.Diagnostics.Debug.WriteLine($"LoadMeters: загружено {list.Count} счетчиков для объекта {_selectedObject.Id}");

                foreach (MeterDto meter in list)
                {
                    Meters.Add(meter);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки счетчиков: {ex.Message}");
            }
        }

        private void LoadHistory()
        {
            try
            {
                Readings.Clear();
                SeriesCollection.Clear();

                if (_selectedMeter == null)
                {
                    return;
                }

                List<MeterReadingHistoryDto> history = _readingRepository.GetHistoryByMeterId(_selectedMeter.Id);

                List<MeterReadingHistoryDto> filtered = history.Where(h =>
                    h.ReadingDate >= _startDate &&
                    h.ReadingDate <= _endDate)
                    .OrderBy(h => h.ReadingDate)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"LoadHistory: загружено {filtered.Count} записей");

                foreach (MeterReadingHistoryDto item in filtered)
                {
                    Readings.Add(item);
                }

                // Обновляем график
                UpdateChart(filtered);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private void UpdateChart(List<MeterReadingHistoryDto> data)
        {
            SeriesCollection.Clear();

            if (!data.Any())
            {
                return;
            }

            double[] values = data.Select(h => (double)h.Value).ToArray();
            double[] consumptions = data.Select(h => (double)(h.Consumption ?? 0)).ToArray();

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