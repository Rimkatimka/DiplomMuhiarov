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

        public decimal ReadingValue
        {
            get => _readingValue;
            set
            {
                if (SetProperty(ref _readingValue, value))
                {
                    CheckAnomaly();
                    RefreshSaveCommand();
                }
            }
        }

        public string WarningMessage
        {
            get => _warningMessage;
            set
            {
                if (SetProperty(ref _warningMessage, value))
                    OnPropertyChanged(nameof(HasWarning));
            }
        }

        public MeterReadingHistoryDto LastReading
        {
            get => _lastReading;
            set
            {
                if (SetProperty(ref _lastReading, value))
                    OnPropertyChanged(nameof(HasLastReading));
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    UpdatePeriodDisplay();
                    CheckAnomaly();
                    RefreshSaveCommand();
                    _ = LoadCurrentPeriodReadingAsync();
                }
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    OnPropertyChanged(nameof(SelectedMonthIndex));
                    UpdatePeriodDisplay();
                    CheckAnomaly();
                    RefreshSaveCommand();
                    _ = LoadCurrentPeriodReadingAsync();
                }
            }
        }

        public int SelectedMonthIndex
        {
            get => SelectedMonth - 1;
            set => SelectedMonth = value + 1;
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (SetProperty(ref _selectedObject, value))
                {
                    OnPropertyChanged(nameof(HasSelectedObject));
                    RefreshSaveCommand();
                    if (value != null && value.Id > 0)
                        _ = LoadMetersAsync(value.Id);
                    else
                        Meters.Clear();
                    _ = LoadReadingHistoryAsync();
                }
            }
        }

        public MeterForReadingDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                if (SetProperty(ref _selectedMeter, value))
                {
                    ReadingValue = 0;
                    WarningMessage = string.Empty;
                    RefreshSaveCommand();
                    OnPropertyChanged(nameof(HasSelectedMeter));
                    SetDefaultPeriod();
                    _ = LoadLastReadingAsync();
                    _ = LoadReadingHistoryAsync();
                    _ = LoadCurrentPeriodReadingAsync();
                    CheckAnomaly();
                }
            }
        }

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand SetLastReadingCommand { get; }

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

        /// <summary>
        /// Дата показания: для текущего месяца — сегодня, для прошлых периодов — последний день месяца.
        /// </summary>
        private DateTime GetReadingDateForPeriod()
        {
            var today = DateTime.Today;
            if (SelectedYear == today.Year && SelectedMonth == today.Month)
                return today;

            return new DateTime(SelectedYear, SelectedMonth, DateTime.DaysInMonth(SelectedYear, SelectedMonth));
        }

        private bool IsFuturePeriod()
        {
            var today = DateTime.Today;
            return SelectedYear > today.Year
                || (SelectedYear == today.Year && SelectedMonth > today.Month);
        }

        private void RefreshSaveCommand() => SaveCommand?.RaiseCanExecuteChanged();

        private async Task LoadObjectsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var objects = await _objectRepository.GetAllAsync();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Objects.Clear();
                    FilteredObjects.Clear();
                    foreach (var obj in objects)
                    {
                        Objects.Add(obj);
                        FilteredObjects.Add(obj);
                    }
                });
            }, "Ошибка загрузки объектов");
        }

        private void ApplyFilter()
        {
            FilteredObjects.Clear();
            var source = string.IsNullOrWhiteSpace(SearchText)
                ? Objects
                : new ObservableCollection<ConsumptionObjectDto>(
                    Objects.Where(o => o.Address.ToLower().Contains(SearchText.ToLower())));
            foreach (var obj in source)
                FilteredObjects.Add(obj);
        }

        private async Task LoadMetersAsync(int objectId)
        {
            try
            {
                var meters = await _meterRepository.GetMetersForReadingAsync(objectId);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Meters.Clear();
                    foreach (var m in meters)
                        Meters.Add(m);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки счетчиков: {ex.Message}";
            }
        }

        private async Task LoadLastReadingAsync()
        {
            if (SelectedMeter == null)
            {
                LastReading = null;
                return;
            }

            try
            {
                var history = await _readingRepository.GetHistoryByMeterIdAsync(SelectedMeter.Id);
                var last = history.OrderByDescending(h => h.ReadingDate).FirstOrDefault();

                if (last == null && SelectedMeter.InitialReading > 0)
                {
                    last = new MeterReadingHistoryDto
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

                LastReading = last;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки последнего показания: {ex.Message}";
            }
        }

        private async Task LoadReadingHistoryAsync()
        {
            if (SelectedMeter == null)
            {
                ReadingHistory.Clear();
                OnPropertyChanged(nameof(HasReadingHistory));
                return;
            }

            try
            {
                var history = await _readingRepository.GetHistoryByMeterIdAsync(SelectedMeter.Id);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ReadingHistory.Clear();
                    foreach (var item in history.OrderByDescending(h => h.ReadingDate).Take(6))
                        ReadingHistory.Add(item);
                    OnPropertyChanged(nameof(HasReadingHistory));
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки истории: {ex.Message}";
            }
        }

        private async Task LoadCurrentPeriodReadingAsync()
        {
            if (SelectedMeter == null)
            {
                ReadingValue = 0;
                return;
            }

            try
            {
                var existing = await _readingRepository.GetByMeterAndPeriodAsync(
                    SelectedMeter.Id, SelectedYear, SelectedMonth);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ReadingValue = existing?.Value ?? 0;
                    if (existing != null)
                        WarningMessage = "Загружено существующее показание за выбранный период. Измените значение и сохраните.";
                    else if (string.IsNullOrEmpty(WarningMessage) || (!WarningMessage.StartsWith("⚠") && !WarningMessage.StartsWith("Подставлено")))
                        WarningMessage = string.Empty;
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки показания за период: {ex.Message}";
            }
        }

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
            if (LastReading == null)
            {
                if (string.IsNullOrEmpty(WarningMessage) || WarningMessage.StartsWith("⚠"))
                    WarningMessage = string.Empty;
                RefreshSaveCommand();
                return;
            }

            decimal diff = ReadingValue - LastReading.Value;
            if (diff < 0)
                WarningMessage = "⚠ Ошибка! Новое показание меньше предыдущего!";
            else if (diff > 1000)
                WarningMessage = "⚠ Внимание! Аномально высокое потребление!";
            else if (!WarningMessage.StartsWith("Подставлено"))
                WarningMessage = string.Empty;

            RefreshSaveCommand();
        }

        private bool CanSave()
        {
            if (SelectedMeter == null || ReadingValue <= 0) return false;

            if (IsFuturePeriod())
            {
                WarningMessage = "Нельзя вводить показания за будущий период";
                return false;
            }

            var today = DateTime.Today;
            if (SelectedYear == today.Year && SelectedMonth == today.Month && today.Day < 15)
            {
                WarningMessage = "Показания за текущий месяц можно вводить с 15-го числа";
                return false;
            }

            if (LastReading != null && ReadingValue < LastReading.Value)
                return false;

            return true;
        }

        private async Task SaveReadingAsync()
        {
            if (!CanSave()) return;

            await ExecuteAsync(async () =>
            {
                var readingDate = GetReadingDateForPeriod();
                var dto = new MeterReadingInputDto
                {
                    MeterId = SelectedMeter.Id,
                    ReadingDate = readingDate,
                    Value = ReadingValue,
                    EnteredByUserId = _currentUser.Id,
                    ReadingStatusId = 1,
                    TariffZone = 1
                };

                await _readingRepository.SaveOrUpdateAsync(dto);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(
                        $"Показания за {Months[SelectedMonth - 1]} {SelectedYear} сохранены ({readingDate:dd.MM.yyyy})",
                        "Успех"));

                await LoadLastReadingAsync();
                await LoadReadingHistoryAsync();
                await LoadCurrentPeriodReadingAsync();
                RefreshSaveCommand();
            }, "Ошибка при сохранении");
        }

        private void ClearForm()
        {
            SearchText = string.Empty;
            SelectedObject = null;
            SelectedMeter = null;
            ReadingValue = 0;
            WarningMessage = string.Empty;
            Meters.Clear();
            ReadingHistory.Clear();
            OnPropertyChanged(nameof(HasReadingHistory));
            SetDefaultPeriod();
            _ = LoadObjectsAsync();
            RefreshSaveCommand();
        }
    }
}
