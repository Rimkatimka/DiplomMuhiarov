using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Data.Repositories.EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EnergyMeteringSystem.App.ViewModels.Meters
{
    public class MeterEditViewModel : ViewModelBase
    {
        private readonly MeterRepository _meterRepository;
        private readonly IMeterTypeRepository _meterTypeRepository;
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterStatusRepository _statusRepository;
        private const decimal MAX_INITIAL_READING = 99999.999m;
        private const decimal MIN_INITIAL_READING = 0;

        private MeterDto _meter;
        private MeterTypeDto _selectedMeterType;
        private ConsumptionObjectDto _selectedObject;
        private MeterStatusDto _selectedStatus;
        private string _serialNumber;
        private decimal _initialReading;
        private int? _serviceLifeYears;
        private int _verificationIntervalYears = 16;
        private DateTime? _installationDate;
        private DateTime? _lastVerificationDate;
        private DateTime? _nextVerificationDate;
        private DateTime? _removalDate;
        private string _dateError;

        public event EventHandler OnMeterSaved;

        public ObservableCollection<MeterTypeDto> MeterTypes { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterStatusDto> Statuses { get; set; }
        public bool IsObjectEnabled => !IsObjectReadOnly;
        public bool HasDateError => !string.IsNullOrEmpty(DateError);

        public bool IsObjectReadOnly { get; private set; }
        public bool IsEditMode { get; private set; }

        // Ограничения для DatePicker
        public DateTime MinInstallationDate => DateTime.Today.AddYears(-100);
        public DateTime MaxInstallationDate => DateTime.Today;
        public DateTime? MinLastVerificationDate => InstallationDate;
        public DateTime? MaxLastVerificationDate => DateTime.Today;

        public DateTime? MinNextVerificationDate
        {
            get
            {
                if (LastVerificationDate.HasValue)
                    return LastVerificationDate.Value.AddMonths(3);
                if (InstallationDate.HasValue)
                    return InstallationDate.Value.AddMonths(3);
                return DateTime.Today.AddMonths(3);
            }
        }

        public DateTime? MaxNextVerificationDate
        {
            get
            {
                if (LastVerificationDate.HasValue)
                    return LastVerificationDate.Value.AddYears(1);
                if (InstallationDate.HasValue)
                    return InstallationDate.Value.AddYears(1);
                return DateTime.Today.AddYears(1);
            }
        }
        
        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }

        public decimal InitialReading
        {
            get => _initialReading;
            set
            {
                if (value < MIN_INITIAL_READING)
                {
                    ToastNotificationService.ShowNear(null, "Начальные показания не могут быть отрицательными", 2000);
                    SetProperty(ref _initialReading, 0);
                }
                else if (value > MAX_INITIAL_READING)
                {
                    ToastNotificationService.ShowNear(null, $"Начальные показания не могут превышать {MAX_INITIAL_READING}", 2000);
                    SetProperty(ref _initialReading, MAX_INITIAL_READING);
                }
                else
                {
                    SetProperty(ref _initialReading, value);
                }

                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public int? ServiceLifeYears
        {
            get => _serviceLifeYears;
            set
            {
                SetProperty(ref _serviceLifeYears, value);
                CalculateRemovalDate();
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? InstallationDate
        {
            get => _installationDate;
            set
            {
                if (SetProperty(ref _installationDate, value))
                {
                    CalculateRemovalDate();
                    OnPropertyChanged(nameof(MinLastVerificationDate));
                    OnPropertyChanged(nameof(MinNextVerificationDate));
                    OnPropertyChanged(nameof(MaxNextVerificationDate));
                    ValidateDates();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? LastVerificationDate
        {
            get => _lastVerificationDate;
            set
            {
                if (SetProperty(ref _lastVerificationDate, value))
                {
                    if (value.HasValue && _verificationIntervalYears > 0)
                    {
                        NextVerificationDate = value.Value.AddYears(_verificationIntervalYears);
                    }
                    OnPropertyChanged(nameof(MinNextVerificationDate));
                    OnPropertyChanged(nameof(MaxNextVerificationDate));
                    ValidateDates();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? NextVerificationDate
        {
            get => _nextVerificationDate;
            set
            {
                if (SetProperty(ref _nextVerificationDate, value))
                {
                    ValidateDates();
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? RemovalDate
        {
            get => _removalDate;
            set => SetProperty(ref _removalDate, value);
        }

        public string DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

        public MeterTypeDto SelectedMeterType
        {
            get => _selectedMeterType;
            set
            {
                SetProperty(ref _selectedMeterType, value);
                if (value != null)
                {
                    ServiceLifeYears = value.ServiceLifeYears;
                    _verificationIntervalYears = value.VerificationIntervalYears ?? 16;

                    if (LastVerificationDate.HasValue)
                    {
                        NextVerificationDate = LastVerificationDate.Value.AddYears(_verificationIntervalYears);
                    }
                }
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
        }

        public MeterStatusDto SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public RelayCommand SaveCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        // Конструктор для добавления
        public MeterEditViewModel(ConsumptionObjectDto currentObject = null)
        {
            System.Diagnostics.Debug.WriteLine($"=== КОНСТРУКТОР: currentObject = {(currentObject != null ? currentObject.Address : "null")} ===");

            _meterRepository = new MeterRepository();
            _meterTypeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();

            if (currentObject != null)
            {
                SelectedObject = Objects.FirstOrDefault(o => o.Id == currentObject.Id);
                IsObjectReadOnly = true;
            }
            else
            {
                IsObjectReadOnly = false;
            }

            InstallationDate = DateTime.Today;
            LastVerificationDate = InstallationDate;  // первая поверка = дата установки
        }

        // Конструктор для редактирования
        public MeterEditViewModel(MeterDto existingMeter)
        {
            System.Diagnostics.Debug.WriteLine($"=== КОНСТРУКТОР РЕДАКТИРОВАНИЯ: MeterId={existingMeter?.Id} ===");

            _meterRepository = new MeterRepository();
            _meterTypeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();

            _meter = existingMeter;
            IsObjectReadOnly = false;
            IsEditMode = true;

            LoadMeter(existingMeter);
        }

        private void LoadData()
        {
            var types = _meterTypeRepository.GetAll();
            MeterTypes.Clear();
            foreach (var item in types)
            {
                MeterTypes.Add(item);
            }

            var objects = _objectRepository.GetAll();
            Objects.Clear();
            foreach (var obj in objects)
                Objects.Add(obj);

            var statuses = _statusRepository.GetAll();
            Statuses.Clear();
            foreach (var item in statuses)
            {
                Statuses.Add(new MeterStatusDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    CanAcceptReadings = true
                });
            }
        }

        private void LoadMeter(MeterDto meter)
        {
            SerialNumber = meter.SerialNumber;
            InstallationDate = meter.InstallationDate;
            InitialReading = meter.InitialReading;
            LastVerificationDate = meter.LastVerificationDate;
            NextVerificationDate = meter.NextVerificationDate;
            ServiceLifeYears = meter.ServiceLifeYears;

            SelectedMeterType = MeterTypes.FirstOrDefault(t => t.Id == meter.MeterTypeId);
            SelectedObject = Objects.FirstOrDefault(o => o.Id == meter.ConsumptionObjectId);
            SelectedStatus = Statuses.FirstOrDefault(s => s.Id == meter.StatusId);
        }

        private void CalculateRemovalDate()
        {
            if (InstallationDate.HasValue && ServiceLifeYears.HasValue)
                RemovalDate = InstallationDate.Value.AddYears(ServiceLifeYears.Value);
        }

        private void ValidateDates()
        {
            DateError = string.Empty;

            // 1. Дата установки не может быть в будущем
            if (InstallationDate > DateTime.Today)
            {
                DateError = "Дата установки не может быть позже сегодняшнего дня";
                return;
            }

            // 2. Последняя поверка не может быть в будущем
            if (LastVerificationDate.HasValue && LastVerificationDate > DateTime.Today)
            {
                DateError = "Дата последней поверки не может быть позже сегодняшнего дня";
                return;
            }

            // 3. Следующая поверка должна быть ПОСЛЕ последней
            if (LastVerificationDate.HasValue && NextVerificationDate.HasValue &&
                NextVerificationDate <= LastVerificationDate)
            {
                DateError = "Дата следующей поверки должна быть позже даты последней поверки";
                return;
            }

            // 4. Следующая поверка не может быть позже даты изъятия
            if (RemovalDate.HasValue && NextVerificationDate.HasValue &&
                NextVerificationDate > RemovalDate)
            {
                DateError = "Дата следующей поверки не может быть позже даты изъятия счетчика";
                return;
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(SerialNumber) &&
                   SelectedMeterType != null &&
                   SelectedObject != null &&
                   SelectedStatus != null &&
                   InstallationDate.HasValue &&
                   string.IsNullOrEmpty(DateError);
        }

        private void Save()
        {
            ValidateDates();

            if (!string.IsNullOrEmpty(DateError))
            {
                MessageBox.Show("Исправьте ошибки в датах перед сохранением",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new MeterDto
            {
                Id = _meter?.Id ?? 0,
                SerialNumber = SerialNumber,
                MeterTypeId = SelectedMeterType.Id,
                ConsumptionObjectId = SelectedObject.Id,
                InstallationDate = InstallationDate.Value,
                InitialReading = InitialReading,
                LastVerificationDate = LastVerificationDate,
                NextVerificationDate = NextVerificationDate,
                ServiceLifeYears = ServiceLifeYears,
                StatusId = SelectedStatus.Id
            };

            if (IsEditMode)
                _meterRepository.Update(dto);
            else
                _meterRepository.Add(dto);

            OnMeterSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnMeterSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}