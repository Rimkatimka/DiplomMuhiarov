using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Meters
{
    public class MeterEditViewModel : EditViewModelBase<MeterDto, MeterRepository>
    {
        private readonly IMeterTypeRepository _meterTypeRepository;
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterStatusRepository _statusRepository;

        private const decimal MAX_INITIAL_READING = 99999.999m;
        private const decimal MIN_INITIAL_READING = 0;

        private MeterTypeDto _selectedMeterType;
        private ConsumptionObjectDto _selectedObject;
        private MeterStatusDto _selectedStatus;
        private string _serialNumber;
        private decimal _initialReading;
        private int? _serviceLifeYears;
        private DateTime? _installationDate;
        private DateTime? _lastVerificationDate;
        private DateTime? _nextVerificationDate;
        private DateTime? _removalDate;
        private string _dateError;

        public ObservableCollection<MeterTypeDto> MeterTypes { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterStatusDto> Statuses { get; set; }

        public bool IsObjectEnabled => !IsObjectReadOnly;
        public bool HasDateError => !string.IsNullOrEmpty(DateError);
        public bool IsObjectReadOnly { get; private set; }

        public DateTime MinInstallationDate => DateTime.Today.AddYears(-100);
        public DateTime MaxInstallationDate => DateTime.Today;

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
                    OnPropertyChanged(nameof(MinNextVerificationDate));
                    OnPropertyChanged(nameof(MaxNextVerificationDate));
                    ValidateDates();
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
                    CalculateNextVerificationDate();
                    ValidateDates();
                }
            }
        }

        public DateTime? NextVerificationDate
        {
            get => _nextVerificationDate;
            set
            {
                SetProperty(ref _nextVerificationDate, value);
                ValidateDates();
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
                    CalculateNextVerificationDate();
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

        // Конструктор для добавления
        public MeterEditViewModel(ConsumptionObjectDto currentObject = null)
            : base(new MeterRepository(), null)
        {
            _meterTypeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            Title = "Регистрация счетчика";
            InstallationDate = DateTime.Today;
            LastVerificationDate = InstallationDate;

            if (currentObject != null)
            {
                IsObjectReadOnly = true;
            }

            _ = LoadDataAsync();
        }

        // Конструктор для редактирования
        public MeterEditViewModel(MeterDto existingMeter)
            : base(new MeterRepository(), existingMeter)
        {
            _meterTypeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            Title = "Редактирование счетчика";
            IsObjectReadOnly = false;

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var types = await _meterTypeRepository.GetAllAsync();
                MeterTypes.Clear();
                foreach (var type in types)
                    MeterTypes.Add(type);

                var objects = await _objectRepository.GetAllAsync();
                Objects.Clear();
                foreach (var obj in objects)
                    Objects.Add(obj);

                var statuses = await _statusRepository.GetAllAsync();
                Statuses.Clear();
                foreach (var status in statuses)
                {
                    Statuses.Add(new MeterStatusDto
                    {
                        Id = status.Id,
                        Name = status.Name,
                        Description = status.Description,
                        CanAcceptReadings = true
                    });
                }

                if (IsEditMode && _originalItem != null)
                {
                    LoadItem(_originalItem);

                    // Выбираем объект после загрузки
                    if (_originalItem.ConsumptionObjectId > 0)
                    {
                        SelectedObject = Objects.FirstOrDefault(o => o.Id == _originalItem.ConsumptionObjectId);
                    }
                }
            }, "Ошибка загрузки данных");
        }

        protected override void LoadItem(MeterDto item)
        {
            SerialNumber = item.SerialNumber;
            InstallationDate = item.InstallationDate;
            InitialReading = item.InitialReading;
            LastVerificationDate = item.LastVerificationDate;
            NextVerificationDate = item.NextVerificationDate;
            ServiceLifeYears = item.ServiceLifeYears;

            SelectedMeterType = MeterTypes.FirstOrDefault(t => t.Id == item.MeterTypeId);
            SelectedStatus = Statuses.FirstOrDefault(s => s.Id == item.StatusId);
        }

        protected override MeterDto GetDto()
        {
            return new MeterDto
            {
                Id = _originalItem?.Id ?? 0,
                SerialNumber = SerialNumber,
                MeterTypeId = SelectedMeterType?.Id ?? 0,
                ConsumptionObjectId = SelectedObject?.Id ?? 0,
                InstallationDate = InstallationDate ?? DateTime.Today,
                InitialReading = InitialReading,
                LastVerificationDate = LastVerificationDate,
                NextVerificationDate = NextVerificationDate,
                ServiceLifeYears = ServiceLifeYears,
                StatusId = SelectedStatus?.Id ?? 1
            };
        }

        protected override async Task SaveToRepositoryAsync(MeterDto dto)
        {
            ValidateDates();

            if (!string.IsNullOrEmpty(DateError))
            {
                MessageBox.Show("Исправьте ошибки в датах перед сохранением",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEditMode)
                await _repository.UpdateAsync(dto);
            else
                await _repository.AddAsync(dto);
        }

        protected override bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(SerialNumber) &&
                   SelectedMeterType != null &&
                   SelectedObject != null &&
                   SelectedStatus != null &&
                   InstallationDate.HasValue &&
                   string.IsNullOrEmpty(DateError);
        }

        private void CalculateRemovalDate()
        {
            if (InstallationDate.HasValue && ServiceLifeYears.HasValue)
                RemovalDate = InstallationDate.Value.AddYears(ServiceLifeYears.Value);
        }

        private void CalculateNextVerificationDate()
        {
            if (LastVerificationDate.HasValue && SelectedMeterType != null)
            {
                int interval = SelectedMeterType.VerificationIntervalYears ?? 16;
                NextVerificationDate = LastVerificationDate.Value.AddYears(interval);
            }
            else if (InstallationDate.HasValue && SelectedMeterType != null)
            {
                int interval = SelectedMeterType.VerificationIntervalYears ?? 16;
                NextVerificationDate = InstallationDate.Value.AddYears(interval);
            }

            OnPropertyChanged(nameof(NextVerificationDate));
        }

        private void ValidateDates()
        {
            DateError = string.Empty;

            if (InstallationDate > DateTime.Today)
            {
                DateError = "Дата установки не может быть позже сегодняшнего дня";
                return;
            }

            if (LastVerificationDate.HasValue && LastVerificationDate > DateTime.Today)
            {
                DateError = "Дата последней поверки не может быть позже сегодняшнего дня";
                return;
            }

            if (LastVerificationDate.HasValue && NextVerificationDate.HasValue &&
                NextVerificationDate <= LastVerificationDate)
            {
                DateError = "Дата следующей поверки должна быть позже даты последней поверки";
                return;
            }

            if (RemovalDate.HasValue && NextVerificationDate.HasValue &&
                NextVerificationDate > RemovalDate)
            {
                DateError = "Дата следующей поверки не может быть позже даты изъятия счетчика";
                return;
            }
        }
    }
}