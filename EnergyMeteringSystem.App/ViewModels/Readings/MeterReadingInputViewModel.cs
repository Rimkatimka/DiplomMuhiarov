using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingInputViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterReadingRepository _readingRepository;
        private readonly MeterRepository _meterRepository;
        private readonly UserDto _currentUser;

        private string _searchText;
        private ConsumptionObjectDto _selectedObject;
        private MeterForReadingDto _selectedMeter;
        private int _selectedYear;
        private int _selectedMonth;
        private decimal _readingValue;
        private string _warningMessage;
        private MeterReadingHistoryDto _lastReading;
        private string _periodDisplay;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredObjects { get; set; }
        public ObservableCollection<MeterForReadingDto> Meters { get; set; }
        public ObservableCollection<MeterReadingHistoryDto> ReadingHistory { get; set; }

        public bool HasSelectedObject => SelectedObject != null;
        public bool HasSelectedMeter => SelectedMeter != null;
        public bool HasLastReading => LastReading != null;
        public bool HasReadingHistory => ReadingHistory?.Count > 0;
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);

        public string PeriodDisplay { get => _periodDisplay; set => SetProperty(ref _periodDisplay, value); }
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
        public decimal ReadingValue { get => _readingValue; set { SetProperty(ref _readingValue, value); CheckAnomaly(); } }
        public string WarningMessage { get => _warningMessage; set { SetProperty(ref _warningMessage, value); OnPropertyChanged(nameof(HasWarning)); } }
        public MeterReadingHistoryDto LastReading { get => _lastReading; set { SetProperty(ref _lastReading, value); OnPropertyChanged(nameof(HasLastReading)); } }

        public int SelectedYear { get => _selectedYear; set { SetProperty(ref _selectedYear, value); UpdatePeriodDisplay(); CheckAnomaly(); } }
        public int SelectedMonth { get => _selectedMonth; set { SetProperty(ref _selectedMonth, value); UpdatePeriodDisplay(); CheckAnomaly(); } }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                OnPropertyChanged(nameof(HasSelectedObject));
                if (value != null && value.Id > 0)
                {
                    _ = LoadMetersAsync(value.Id);
                }
                _ = LoadReadingHistoryAsync();
            }
        }

        public MeterForReadingDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                SetProperty(ref _selectedMeter, value);
                OnPropertyChanged(nameof(HasSelectedMeter));
                SetDefaultPeriod();
                _ = LoadLastReadingAsync();
                _ = LoadReadingHistoryAsync();
                CheckAnomaly();
            }
        }

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand SetLastReadingCommand { get; }  // ← RelayCommand (не async)

        public MeterReadingInputViewModel(UserDto currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _objectRepository = new ConsumptionObjectRepository();
            _readingRepository = new MeterReadingRepository();
            _meterRepository = new MeterRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>();
            Meters = new ObservableCollection<MeterForReadingDto>();
            ReadingHistory = new ObservableCollection<MeterReadingHistoryDto>();

            SaveCommand = new AsyncRelayCommand(async () => await SaveReadingAsync(), () => CanSave());
            ClearCommand = new RelayCommand(_ => ClearForm());
            SetLastReadingCommand = new RelayCommand(_ => SetLastReadingValue(), _ => HasLastReading); 

            InitializeYearsAndMonths();
            _ = LoadObjectsAsync();

            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;
            SetDefaultPeriod();
        }

        private void InitializeYearsAndMonths()
        {
            for (int i = 2020; i <= DateTime.Today.Year + 1; i++) Years.Add(i);
            foreach (var m in new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" })
                Months.Add(m);
        }

        private void SetDefaultPeriod()
        {
            var today = DateTime.Today;
            if (today.Day >= 15)
            {
                SelectedYear = today.Year;
                SelectedMonth = today.Month;
            }
            else
            {
                var prev = today.AddMonths(-1);
                SelectedYear = prev.Year;
                SelectedMonth = prev.Month;
            }
            UpdatePeriodDisplay();
        }

        private void UpdatePeriodDisplay() => PeriodDisplay = $"{Months[SelectedMonth - 1]} {SelectedYear}";

        private async Task LoadObjectsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var objects = await _objectRepository.GetAllAsync();
                Objects.Clear();
                FilteredObjects.Clear();
                foreach (var obj in objects)
                {
                    Objects.Add(obj);
                    FilteredObjects.Add(obj);
                }
            }, "Ошибка загрузки объектов");
        }

        private void ApplyFilter()
        {
            FilteredObjects.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Objects
                : new ObservableCollection<ConsumptionObjectDto>(Objects.Where(o => o.Address.ToLower().Contains(SearchText.ToLower())));
            foreach (var obj in filtered) FilteredObjects.Add(obj);
        }

        private async Task LoadMetersAsync(int objectId)
        {
            await ExecuteAsync(async () =>
            {
                var meters = await _meterRepository.GetMetersForReadingAsync(objectId);
                Meters.Clear();
                foreach (var m in meters) Meters.Add(m);
            }, "Ошибка загрузки счетчиков");
        }

        private async Task LoadLastReadingAsync()
        {
            if (SelectedMeter == null) { LastReading = null; return; }

            await ExecuteAsync(async () =>
            {
                var history = await _readingRepository.GetHistoryByMeterIdAsync(SelectedMeter.Id);
                LastReading = history.OrderByDescending(h => h.ReadingDate).FirstOrDefault();

                if (LastReading == null && SelectedMeter.InitialReading > 0)
                {
                    LastReading = new MeterReadingHistoryDto
                    {
                        Id = 0,
                        ReadingDate = SelectedMeter.InstallationDate ?? DateTime.Now.AddMonths(-1),
                        Value = SelectedMeter.InitialReading,
                        Consumption = 0,
                        StatusName = "Начальное",
                        EnteredBy = "Система",
                        EnteredAt = DateTime.Now
                    };
                }
            }, "Ошибка загрузки последнего показания");
        }

        private async Task LoadReadingHistoryAsync()
        {
            if (SelectedMeter == null) { ReadingHistory.Clear(); return; }

            await ExecuteAsync(async () =>
            {
                var history = await _readingRepository.GetHistoryByMeterIdAsync(SelectedMeter.Id);
                ReadingHistory.Clear();
                foreach (var item in history.OrderByDescending(h => h.ReadingDate).Take(6))
                    ReadingHistory.Add(item);
            }, "Ошибка загрузки истории");
        }

        // ✅ ЭТОТ МЕТОД СИНХРОННЫЙ — для кнопки "Подставить"
        private void SetLastReadingValue()
        {
            if (LastReading != null)
            {
                ReadingValue = LastReading.Value;
                WarningMessage = "Подставлено последнее показание. При необходимости отредактируйте.";
            }
        }

        private void CheckAnomaly()
        {
            if (LastReading == null) return;
            decimal diff = ReadingValue - LastReading.Value;
            if (diff < 0) WarningMessage = "⚠ Ошибка! Новое показание меньше предыдущего!";
            else if (diff > 1000) WarningMessage = "⚠ Внимание! Аномально высокое потребление!";
            else WarningMessage = string.Empty;
        }

        private bool CanSave()
        {
            if (SelectedMeter == null || ReadingValue <= 0) return false;

            var readingDate = new DateTime(SelectedYear, SelectedMonth, 1);
            var today = DateTime.Today;

            if (readingDate > today) { WarningMessage = "Нельзя вводить показания за будущий период"; return false; }
            if (readingDate.Year == today.Year && readingDate.Month == today.Month && today.Day < 15)
            { WarningMessage = "Показания за текущий месяц можно вводить с 15-го числа"; return false; }

            return true;
        }

        private async Task SaveReadingAsync()
        {
            await ExecuteAsync(async () =>
            {
                if (SelectedMeter == null) throw new Exception("Выберите счетчик");

                var readingDate = new DateTime(SelectedYear, SelectedMonth, 1);
                var dto = new MeterReadingInputDto
                {
                    MeterId = SelectedMeter.Id,
                    ReadingDate = readingDate,
                    Value = ReadingValue,
                    EnteredByUserId = _currentUser.Id,
                    ReadingStatusId = 1,
                    TariffZone = 1
                };
                await _readingRepository.AddAsync(dto);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show($"Показания за {Months[SelectedMonth - 1]} {SelectedYear} успешно сохранены", "Успех"));
                ClearForm();
            }, "Ошибка при сохранении");
        }

        private void ClearForm()
        {
            SearchText = string.Empty;
            SelectedObject = null;
            SelectedMeter = null;
            ReadingValue = 0;
            WarningMessage = string.Empty;
            SetDefaultPeriod();
            _ = LoadObjectsAsync();
            _ = LoadLastReadingAsync();
            _ = LoadReadingHistoryAsync();
        }
    }
}