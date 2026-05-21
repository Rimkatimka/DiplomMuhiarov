using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<MeterReadingHistoryDto> _readingHistory;
        private string _periodDisplay;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredObjects { get; set; }
        public ObservableCollection<MeterForReadingDto> Meters { get; set; }

        public ObservableCollection<MeterReadingHistoryDto> ReadingHistory
        {
            get => _readingHistory;
            set => SetProperty(ref _readingHistory, value);
        }

        // Вспомогательные свойства для Visibility
        public bool HasSelectedObject => SelectedObject != null;
        public bool HasSelectedMeter => SelectedMeter != null;
        public bool HasLastReading => LastReading != null;
        public bool HasReadingHistory => ReadingHistory?.Count > 0;
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);

        public string PeriodDisplay
        {
            get => _periodDisplay;
            set => SetProperty(ref _periodDisplay, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                OnPropertyChanged(nameof(HasSelectedObject));
                if (value != null && value.Id > 0)
                {
                    LoadMeters(value.Id);
                }
                LoadReadingHistory();
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
                LoadLastReading();
                LoadReadingHistory();
                CheckAnomaly();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                SetProperty(ref _selectedYear, value);
                UpdatePeriodDisplay();
                CheckAnomaly();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                SetProperty(ref _selectedMonth, value);
                UpdatePeriodDisplay();
                CheckAnomaly();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string SelectedMonthName
        {
            get
            {
                if (Months == null || Months.Count == 0 || SelectedMonth < 1 || SelectedMonth > 12)
                    return "";
                return Months[SelectedMonth - 1];
            }
        }

        public decimal ReadingValue
        {
            get => _readingValue;
            set
            {
                SetProperty(ref _readingValue, value);
                CheckAnomaly();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string WarningMessage
        {
            get => _warningMessage;
            set
            {
                SetProperty(ref _warningMessage, value);
                OnPropertyChanged(nameof(HasWarning));
            }
        }

        public MeterReadingHistoryDto LastReading
        {
            get => _lastReading;
            set
            {
                SetProperty(ref _lastReading, value);
                OnPropertyChanged(nameof(HasLastReading));
            }
        }

        public RelayCommand SaveCommand { get; }
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

            SaveCommand = new RelayCommand(_ => SaveReading(), _ => CanSave());
            ClearCommand = new RelayCommand(_ => ClearForm());
            SetLastReadingCommand = new RelayCommand(_ => SetLastReadingValue(), _ => HasLastReading);

            InitializeYearsAndMonths();
            LoadObjects();

            // Устанавливаем начальные значения до вызова SetDefaultPeriod
            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;

            SetDefaultPeriod();
        }

        private void InitializeYearsAndMonths()
        {
            for (int i = 2020; i <= DateTime.Today.Year + 1; i++)
                Years.Add(i);

            Months.Add("Январь");
            Months.Add("Февраль");
            Months.Add("Март");
            Months.Add("Апрель");
            Months.Add("Май");
            Months.Add("Июнь");
            Months.Add("Июль");
            Months.Add("Август");
            Months.Add("Сентябрь");
            Months.Add("Октябрь");
            Months.Add("Ноябрь");
            Months.Add("Декабрь");
        }

        private void SetDefaultPeriod()
        {
            var today = DateTime.Today;

            // Если сегодня 15-е число или позже → текущий месяц
            if (today.Day >= 15)
            {
                SelectedYear = today.Year;
                SelectedMonth = today.Month;
            }
            // Если сегодня 1-14 число → предыдущий месяц
            else
            {
                var previousMonth = today.AddMonths(-1);
                SelectedYear = previousMonth.Year;
                SelectedMonth = previousMonth.Month;
            }

            UpdatePeriodDisplay();
        }

        private void UpdatePeriodDisplay()
        {
            if (SelectedMonth >= 1 && SelectedMonth <= 12 && Months != null && Months.Count >= SelectedMonth)
            {
                PeriodDisplay = $"{Months[SelectedMonth - 1]} {SelectedYear}";
            }
            else
            {
                PeriodDisplay = $"{SelectedMonth}.{SelectedYear}";
            }
        }

        private void LoadObjects()
        {
            var objects = _objectRepository.GetAll();
            Objects.Clear();
            FilteredObjects.Clear();
            foreach (var obj in objects)
            {
                Objects.Add(obj);
                FilteredObjects.Add(obj);
            }
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredObjects.Clear();
                foreach (var obj in Objects)
                    FilteredObjects.Add(obj);
            }
            else
            {
                var lowerSearch = SearchText.ToLower();
                var filtered = Objects.Where(o =>
                    o.Address.ToLower().Contains(lowerSearch)).ToList();
                FilteredObjects.Clear();
                foreach (var obj in filtered)
                    FilteredObjects.Add(obj);
            }
        }

        private DateTime? GetLastReadingDate(int meterId)
        {
            var lastReading = _readingRepository.GetHistoryByMeterId(meterId)
                .OrderByDescending(r => r.ReadingDate)
                .FirstOrDefault();
            return lastReading?.ReadingDate;
        }

        private void LoadMeters(int objectId)
        {
            Meters.Clear();
            var meters = _meterRepository.GetByObjectId(objectId);

            foreach (var m in meters)
            {
                var lastReading = _readingRepository.GetLastReading(m.Id);
                var lastReadingDate = GetLastReadingDate(m.Id);

                Meters.Add(new MeterForReadingDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeName = m.MeterTypeName,
                    LastReading = lastReading,
                    LastReadingDate = lastReadingDate,
                    StatusName = m.StatusName
                });
            }
        }

        private void LoadLastReading()
        {
            if (SelectedMeter == null)
            {
                LastReading = null;
                return;
            }

            var history = _readingRepository.GetHistoryByMeterId(SelectedMeter.Id);
            LastReading = history.OrderByDescending(h => h.ReadingDate).FirstOrDefault();
        }

        private void LoadReadingHistory()
        {
            if (SelectedMeter == null)
            {
                ReadingHistory.Clear();
                OnPropertyChanged(nameof(HasReadingHistory));
                return;
            }

            var history = _readingRepository.GetHistoryByMeterId(SelectedMeter.Id);
            ReadingHistory.Clear();
            foreach (var item in history.OrderByDescending(h => h.ReadingDate).Take(6))
            {
                ReadingHistory.Add(item);
            }
            OnPropertyChanged(nameof(HasReadingHistory));
        }

        private void SetLastReadingValue()
        {
            if (LastReading != null)
            {
                ReadingValue = LastReading.Value;
                WarningMessage = "Подставлено последнее показание. При необходимости отредактируйте.";
            }
        }

        private bool CanSave()
        {
            if (SelectedMeter == null) return false;
            if (ReadingValue <= 0) return false;
            if (HasReadingForSelectedPeriod()) return false;

            var readingDate = new DateTime(SelectedYear, SelectedMonth, 1);
            var today = DateTime.Today;

            // Запрет на будущие месяцы
            if (readingDate > today)
            {
                WarningMessage = "Нельзя вводить показания за будущий период";
                return false;
            }

            // Для текущего месяца: можно вводить только с 15-го числа
            if (readingDate.Year == today.Year && readingDate.Month == today.Month)
            {
                if (today.Day < 15)
                {
                    WarningMessage = "Показания за текущий месяц можно вводить с 15-го числа";
                    return false;
                }
            }

            return true;
        }

        private bool HasReadingForSelectedPeriod()
        {
            if (SelectedMeter == null) return false;

            var history = _readingRepository.GetHistoryByMeterId(SelectedMeter.Id);
            return history.Any(h => h.ReadingDate.Year == SelectedYear &&
                                    h.ReadingDate.Month == SelectedMonth);
        }

        private void CheckAnomaly()
        {
            if (LastReading == null) return;

            decimal difference = ReadingValue - LastReading.Value;

            if (difference < 0)
            {
                WarningMessage = "⚠ Ошибка! Новое показание меньше предыдущего!";
            }
            else if (difference > 1000)
            {
                WarningMessage = "⚠ Внимание! Аномально высокое потребление!";
            }
            else
            {
                WarningMessage = string.Empty;
            }
        }

        private void SaveReading()
        {
            if (SelectedMeter == null)
            {
                MessageBox.Show("Выберите счетчик", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HasReadingForSelectedPeriod())
            {
                MessageBox.Show($"Показания за {SelectedMonthName} {SelectedYear} уже введены.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime readingDate = new DateTime(SelectedYear, SelectedMonth, 1);

            if (readingDate > DateTime.Today)
            {
                MessageBox.Show("Нельзя вводить показания за будущий период", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new MeterReadingInputDto
            {
                MeterId = SelectedMeter.Id,
                ReadingDate = readingDate,
                Value = ReadingValue,
                EnteredByUserId = _currentUser.Id,
                ReadingStatusId = 1,
                TariffZone = 1
            };

            try
            {
                _readingRepository.Add(dto);
                MessageBox.Show($"Показания за {SelectedMonthName} {SelectedYear} успешно сохранены",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            SearchText = string.Empty;
            SelectedObject = null;
            SelectedMeter = null;
            ReadingValue = 0;
            WarningMessage = string.Empty;
            SetDefaultPeriod();
            LoadObjects();
            LoadLastReading();
            LoadReadingHistory();
        }
    }
}