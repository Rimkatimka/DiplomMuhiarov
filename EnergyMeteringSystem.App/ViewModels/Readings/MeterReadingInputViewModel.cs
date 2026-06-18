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

        // НОРМА ПОТРЕБЛЕНИЯ
        private decimal? _normConsumption;
        private decimal? _totalNormConsumption;
        private decimal? _deviation;
        private string _normStatus;

        private MeterReadingHistoryDto _existingReading;
        private bool _isEditMode;

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
        public bool IsEditMode => _isEditMode;

        public decimal? NormConsumption
        {
            get => _normConsumption;
            set => SetProperty(ref _normConsumption, value);
        }

        public decimal? TotalNormConsumption
        {
            get => _totalNormConsumption;
            set => SetProperty(ref _totalNormConsumption, value);
        }

        public decimal? Deviation
        {
            get => _deviation;
            set => SetProperty(ref _deviation, value);
        }

        public string NormStatus
        {
            get => _normStatus;
            set => SetProperty(ref _normStatus, value);
        }

        public string PeriodDisplay { get => _periodDisplay; set => SetProperty(ref _periodDisplay, value); }
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); ApplyFilter(); } }
        public decimal ReadingValue { get => _readingValue; set { SetProperty(ref _readingValue, value); CheckAnomaly(); CheckNormConsumption(); } }
        public string WarningMessage { get => _warningMessage; set { SetProperty(ref _warningMessage, value); OnPropertyChanged(nameof(HasWarning)); } }
        public MeterReadingHistoryDto LastReading { get => _lastReading; set { SetProperty(ref _lastReading, value); OnPropertyChanged(nameof(HasLastReading)); } }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                SetProperty(ref _selectedYear, value);
                UpdatePeriodDisplay();
                _ = CheckExistingReadingAsync();  // ✅ ИСПРАВЛЕНО
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                SetProperty(ref _selectedMonth, value);
                UpdatePeriodDisplay();
                _ = CheckExistingReadingAsync();  // ✅ ИСПРАВЛЕНО
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                OnPropertyChanged(nameof(HasSelectedObject));
                UpdateNormConsumption();

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
                _ = CheckExistingReadingAsync();  // ✅ ИСПРАВЛЕНО
                CheckAnomaly();
                CheckNormConsumption();
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

            SaveCommand = new AsyncRelayCommand(async () => await SaveReadingAsync(), () => true);
            ClearCommand = new RelayCommand(_ => ClearForm());
            SetLastReadingCommand = new RelayCommand(_ => SetLastReadingValue(), _ => HasLastReading);

            InitializeYearsAndMonths();
            _ = LoadObjectsAsync();

            _selectedYear = DateTime.Today.Year;
            _selectedMonth = DateTime.Today.Month;
            SetDefaultPeriod();
        }

        // ✅ ОБНОВЛЕНИЕ НОРМЫ
        private void UpdateNormConsumption()
        {
            if (SelectedObject != null)
            {
                NormConsumption = SelectedObject.NormConsumption;

                if (NormConsumption.HasValue && SelectedObject.ResidentCount.HasValue)
                {
                    TotalNormConsumption = NormConsumption.Value * SelectedObject.ResidentCount.Value;
                }
                else
                {
                    TotalNormConsumption = null;
                }
            }
            else
            {
                NormConsumption = null;
                TotalNormConsumption = null;
            }

            OnPropertyChanged(nameof(NormConsumption));
            OnPropertyChanged(nameof(TotalNormConsumption));

            CheckNormConsumption();
        }

        // ✅ ПРОВЕРКА НОРМЫ
        private void CheckNormConsumption()
        {
            Deviation = null;
            NormStatus = "";

            if (!TotalNormConsumption.HasValue || TotalNormConsumption.Value == 0)
                return;

            var consumption = GetConsumptionForPeriod();
            if (!consumption.HasValue || consumption.Value == 0)
                return;

            Deviation = consumption.Value - TotalNormConsumption.Value;
            var deviationPercent = (Deviation.Value / TotalNormConsumption.Value) * 100;

            if (deviationPercent > 30)
            {
                NormStatus = "⚠ Превышение нормы!";
                WarningMessage = $"⚠ Потребление ({consumption.Value:F2} кВт·ч) превышает норму ({TotalNormConsumption.Value:F2} кВт·ч) на {deviationPercent:F0}%";
            }
            else if (deviationPercent < -30)
            {
                NormStatus = "⚠ Значительно ниже нормы!";
                WarningMessage = $"⚠ Потребление ({consumption.Value:F2} кВт·ч) значительно ниже нормы ({TotalNormConsumption.Value:F2} кВт·ч)";
            }
            else
            {
                NormStatus = "✅ В пределах нормы";
                WarningMessage = "";
            }
        }

        private decimal? GetConsumptionForPeriod()
        {
            if (SelectedMeter == null || ReadingValue <= 0)
                return null;

            if (LastReading != null)
            {
                return ReadingValue - LastReading.Value;
            }

            return null;
        }

        private void InitializeYearsAndMonths()
        {
            var today = DateTime.Today;

            for (int i = today.Year - 1; i <= today.Year + 1; i++)
                Years.Add(i);

            foreach (var m in new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                      "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" })
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

        private void UpdatePeriodDisplay()
        {
            var date = new DateTime(SelectedYear, SelectedMonth, 1);
            var today = DateTime.Today;
            var status = "";

            if (date > today)
                status = " (будущий период)";
            else if (date < today.AddMonths(-3))
                status = " (архивный период)";
            else if (date < today)
                status = " (прошлый период)";
            else
                status = " (текущий период)";

            PeriodDisplay = $"{Months[SelectedMonth - 1]} {SelectedYear}{status}";
        }

        // ✅ ИСПРАВЛЕНО: async Task вместо async void
        private async Task CheckExistingReadingAsync()
        {
            _isEditMode = false;
            _existingReading = null;

            if (SelectedMeter == null) return;

            var readingDate = new DateTime(SelectedYear, SelectedMonth, 1);
            var today = DateTime.Today;

            if (readingDate > today)
            {
                WarningMessage = "⚠ Нельзя вводить показания за будущий период!";
                return;
            }

            if (readingDate < today.AddMonths(-3))
            {
                WarningMessage = "⚠ Можно вводить показания только за последние 3 месяца!";
                return;
            }

            var history = await _readingRepository.GetHistoryByMeterIdAsync(SelectedMeter.Id);
            var existing = history.FirstOrDefault(h =>
                h.ReadingDate.Year == SelectedYear &&
                h.ReadingDate.Month == SelectedMonth);

            if (existing != null)
            {
                _isEditMode = true;
                _existingReading = existing;
                ReadingValue = existing.Value;
                WarningMessage = $"⚠ Показание за {Months[SelectedMonth - 1]} {SelectedYear} уже существует!\n" +
                                $"Текущее значение: {existing.Value:F2}\n" +
                                "Измените значение и сохраните для обновления.";
            }
            else
            {
                WarningMessage = "";
                ReadingValue = 0;
            }
        }

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
            if (diff < 0)
                WarningMessage = "⚠ Ошибка! Новое показание меньше предыдущего!";
            else if (diff > 1000)
                WarningMessage = "⚠ Внимание! Аномально высокое потребление!";
            else if (WarningMessage.Contains("существует") == false)
                WarningMessage = string.Empty;
        }

        private async Task SaveReadingAsync()
        {
            if (SelectedMeter == null)
            {
                MessageBox.Show("Выберите счетчик для ввода показаний", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var readingDate = new DateTime(SelectedYear, SelectedMonth, 1);
            var today = DateTime.Today;

            if (readingDate > today)
            {
                MessageBox.Show("Нельзя вводить показания за будущий период!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (readingDate < today.AddMonths(-3))
            {
                MessageBox.Show("Можно вводить показания только за последние 3 месяца!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ReadingValue <= 0)
            {
                MessageBox.Show("Введите корректное показание (больше 0)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ПРОВЕРКА НА ПРЕВЫШЕНИЕ НОРМЫ
            if (TotalNormConsumption.HasValue && TotalNormConsumption.Value > 0)
            {
                var consumption = GetConsumptionForPeriod();
                if (consumption.HasValue)
                {
                    var deviationPercent = ((consumption.Value - TotalNormConsumption.Value) / TotalNormConsumption.Value) * 100;

                    if (deviationPercent > 50)
                    {
                        var result = MessageBox.Show(
                            $"⚠ ВНИМАНИЕ! Потребление ({consumption.Value:F2} кВт·ч)\n" +
                            $"превышает норму ({TotalNormConsumption.Value:F2} кВт·ч)\n" +
                            $"на {deviationPercent:F0}%!\n\n" +
                            "Проверьте правильность введенных данных.\n\n" +
                            "Продолжить сохранение?",
                            "Превышение нормы потребления",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (result == MessageBoxResult.No)
                            return;
                    }
                }
            }

            if (LastReading != null && ReadingValue < LastReading.Value)
            {
                var result = MessageBox.Show(
                    $"Новое показание ({ReadingValue:F2}) меньше предыдущего ({LastReading.Value:F2})!\n\n" +
                    "Это может указывать на ошибку. Продолжить?",
                    "Проверка данных",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                    return;
            }

            await ExecuteAsync(async () =>
            {
                var dto = new MeterReadingInputDto
                {
                    MeterId = SelectedMeter.Id,
                    ReadingDate = readingDate,
                    Value = ReadingValue,
                    EnteredByUserId = _currentUser.Id,
                    ReadingStatusId = 1,
                    TariffZone = 1
                };

                if (_isEditMode && _existingReading != null)
                {
                    await _readingRepository.UpdateAsync(_existingReading.Id, dto);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        MessageBox.Show($"Показания за {Months[SelectedMonth - 1]} {SelectedYear} успешно обновлены!", "Успех"));
                }
                else
                {
                    await _readingRepository.AddAsync(dto);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        MessageBox.Show($"Показания за {Months[SelectedMonth - 1]} {SelectedYear} успешно сохранены!", "Успех"));
                }

                ClearForm();
                _ = LoadReadingHistoryAsync();
                _ = LoadLastReadingAsync();

            }, "Ошибка при сохранении");
        }

        private void ClearForm()
        {
            SearchText = string.Empty;
            SelectedObject = null;
            SelectedMeter = null;
            ReadingValue = 0;
            WarningMessage = string.Empty;
            _isEditMode = false;
            _existingReading = null;
            NormConsumption = null;
            TotalNormConsumption = null;
            Deviation = null;
            NormStatus = "";
            SetDefaultPeriod();
            _ = LoadObjectsAsync();
            _ = LoadLastReadingAsync();
            _ = LoadReadingHistoryAsync();
        }
    }
}