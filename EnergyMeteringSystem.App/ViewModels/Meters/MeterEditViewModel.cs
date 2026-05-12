using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnergyMeteringSystem.App.ViewModels.Meters
{
    public class MeterEditViewModel : ViewModelBase
    {
        private readonly MeterRepository _meterRepository;
        private readonly MeterTypeRepository _typeRepository;
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterStatusRepository _statusRepository;

        private MeterDto _meter;
        private MeterTypeDto _selectedMeterType;
        private ConsumptionObjectDto _selectedObject;
        private MeterStatusDto _selectedStatus;
        private string _serialNumber;
        private decimal _initialReading;
        private int? _serviceLifeYears;
        private DateTime? _installationDate;
        private DateTime? _verificationDate;
        private DateTime? _nextVerificationDate;
        private DateTime? _removalDate;
        private string _dateError;

        public event EventHandler OnMeterSaved;

        public ObservableCollection<MeterTypeDto> MeterTypes { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterStatusDto> Statuses { get; set; }
        public bool IsObjectEnabled => !IsObjectReadOnly;

        public bool IsObjectReadOnly { get; private set; }
        public bool IsEditMode { get; private set; }

        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }

        public decimal InitialReading
        {
            get => _initialReading;
            set => SetProperty(ref _initialReading, value);
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
                SetProperty(ref _installationDate, value);
                CalculateRemovalDate();
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? VerificationDate
        {
            get => _verificationDate;
            set
            {
                SetProperty(ref _verificationDate, value);
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime? NextVerificationDate
        {
            get => _nextVerificationDate;
            set
            {
                SetProperty(ref _nextVerificationDate, value);
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
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
                if (value != null && value.ServiceLifeYears.HasValue)
                {
                    ServiceLifeYears = value.ServiceLifeYears;
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

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Конструктор для добавления (с фиксированным объектом)
        public MeterEditViewModel(ConsumptionObjectDto currentObject)
        {
            SelectedObject = currentObject;
            IsObjectReadOnly = true;
            IsEditMode = false;
            InstallationDate = DateTime.Today;

            _meterRepository = new MeterRepository();
            _typeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();
        }

        // Конструктор для редактирования
        public MeterEditViewModel(MeterDto existingMeter)
        {
            _meter = existingMeter;
            IsObjectReadOnly = false;
            IsEditMode = true;

            _meterRepository = new MeterRepository();
            _typeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();
            LoadMeter(existingMeter);
        }

        private void LoadData()
        {
            // ✅ Используем _typeRepository (MeterTypeRepository), а не _typeRepository как DirectoryRepository
            var types = _typeRepository.GetAll();  // ← это List<MeterTypeDto>
            foreach (var item in types)
            {
                MeterTypes.Add(item);  // ← просто добавляем, не создаём новый объект
            }

            // Загрузка объектов
            var objects = _objectRepository.GetAll();
            foreach (var obj in objects)
                Objects.Add(obj);

            // Загрузка статусов
            var statuses = _statusRepository.GetAll();
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
            VerificationDate = meter.VerificationDate;
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

        private int GetVerificationIntervalYears(int meterTypeId)
        {
            using (var context = new EnergyMeteringSystemEntities())
            {
                var interval = context.VerificationInterval
                    .FirstOrDefault(vi => vi.MeterTypeId == meterTypeId);
                return interval?.Years ?? 0;
            }
        }

        private void ValidateDates()
        {
            if (InstallationDate > DateTime.Today)
            {
                DateError = "Дата установки не может быть позже сегодняшнего дня";
                return;
            }

            if (VerificationDate.HasValue && NextVerificationDate.HasValue && VerificationDate > NextVerificationDate)
            {
                DateError = "Дата поверки не может быть позже следующей поверки";
                return;
            }

            DateError = string.Empty;
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
            var dto = new MeterDto
            {
                Id = _meter?.Id ?? 0,
                SerialNumber = SerialNumber,
                MeterTypeId = SelectedMeterType.Id,
                ConsumptionObjectId = SelectedObject.Id,
                InstallationDate = InstallationDate.Value,
                InitialReading = InitialReading,
                VerificationDate = VerificationDate,
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