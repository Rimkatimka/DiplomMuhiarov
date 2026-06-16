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
        private bool _isLoading;

        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterDto> Meters { get; set; }
        public ObservableCollection<MeterReadingHistoryDto> Readings { get; set; }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] ChartDates { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

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

            // Загружаем объекты при создании ViewModel
            _ = LoadObjectsAsync();
        }

        private async Task LoadObjectsAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("LoadObjectsAsync: начало");

                var list = await _objectRepository.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadObjectsAsync: загружено {list.Count} объектов");

                Objects.Clear();
                foreach (var obj in list)
                {
                    Objects.Add(obj);
                }

                // Если есть объекты - выбираем первый
                if (Objects.Any())
                {
                    SelectedObject = Objects.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки объектов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMetersAsync()
        {
            try
            {
                IsLoading = true;
                Meters.Clear();

                if (_selectedObject == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadMeters: объект не выбран");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"LoadMeters: загружаем счетчики для объекта {_selectedObject.Id}");
                var list = await _meterRepository.GetByObjectIdAsync(_selectedObject.Id);
                System.Diagnostics.Debug.WriteLine($"LoadMeters: загружено {list.Count} счетчиков");

                foreach (var meter in list)
                {
                    Meters.Add(meter);
                }

                // Если есть счетчики - выбираем первый
                if (Meters.Any())
                {
                    SelectedMeter = Meters.First();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки счетчиков: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                IsLoading = true;
                Readings.Clear();
                SeriesCollection.Clear();

                if (_selectedMeter == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadHistory: счетчик не выбран");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"LoadHistory: загружаем историю для счетчика {_selectedMeter.Id}");
                var history = await _readingRepository.GetHistoryByMeterIdAsync(_selectedMeter.Id);
                System.Diagnostics.Debug.WriteLine($"LoadHistory: загружено {history.Count} записей");

                var filtered = history
                    .Where(h => h.ReadingDate >= _startDate && h.ReadingDate <= _endDate)
                    .OrderBy(h => h.ReadingDate)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"LoadHistory: после фильтрации {filtered.Count} записей");

                foreach (var item in filtered)
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
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateChart(System.Collections.Generic.List<MeterReadingHistoryDto> data)
        {
            SeriesCollection.Clear();

            if (!data.Any())
            {
                System.Diagnostics.Debug.WriteLine("UpdateChart: нет данных для графика");
                return;
            }

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

            System.Diagnostics.Debug.WriteLine($"UpdateChart: график обновлен с {data.Count} точками");
        }
    }
}