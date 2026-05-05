using System;
using System.Collections.ObjectModel;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

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

        public event EventHandler OnMeterSaved;

        public ObservableCollection<MeterTypeDto> MeterTypes { get; set; }
        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<MeterStatusDto> Statuses { get; set; }

        public string SerialNumber { get; set; }
        public DateTime InstallationDate { get; set; }
        public decimal InitialReading { get; set; }
        private DateTime? _verificationDate;
        public DateTime? VerificationDate
        {
            get => _verificationDate;
            set
            {
                SetProperty(ref _verificationDate, value);
                if (value.HasValue && SelectedMeterType != null)
                {
                    // Взять интервал поверки из БД по типу счётчика
                    int years = GetVerificationIntervalYears(SelectedMeterType.Id);
                    NextVerificationDate = value.Value.AddYears(years);
                }
            }
        }
        public MeterTypeDto SelectedMeterType
        {
            get => _selectedMeterType;
            set => SetProperty(ref _selectedMeterType, value);
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

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public MeterEditViewModel(MeterDto existingMeter = null)
        {
            System.Diagnostics.Debug.WriteLine("MeterEditViewModel constructor");

            _meterRepository = new MeterRepository();
            _typeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = [];
            Objects = [];
            Statuses = [];

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            InstallationDate = DateTime.Today;

            LoadData();

            if (existingMeter != null)
            {
                IsEditMode = true;
                LoadMeter(existingMeter);
            }

            System.Diagnostics.Debug.WriteLine("MeterEditViewModel constructor end");
        }

        private void LoadData()
        {
            // Загрузка типов счетчиков - преобразуем DirectoryDto в MeterTypeDto
            System.Collections.Generic.List<DirectoryDto> types = _typeRepository.GetAll(); // это List<DirectoryDto>
            foreach (DirectoryDto item in types)
            {
                MeterTypes.Add(new MeterTypeDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    // Заполните остальные поля значениями по умолчанию
                    Voltage = 220,
                    MaxCurrent = 40,
                    AccuracyClass = "1.0",
                    DigitCount = 6,
                    DecimalPlaces = 0
                });
            }

            // Загрузка объектов
            System.Collections.Generic.List<ConsumptionObjectDto> objects = _objectRepository.GetAll();
            foreach (ConsumptionObjectDto obj in objects)
            {
                Objects.Add(obj);
            }

            // Загрузка статусов
            System.Collections.Generic.List<DirectoryDto> statuses = _statusRepository.GetAll();
            foreach (DirectoryDto item in statuses)
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
            _meter = meter;
            SerialNumber = meter.SerialNumber;
            SelectedMeterType = FindMeterType(meter.MeterTypeId);
            SelectedObject = FindObject(meter.ConsumptionObjectId);
            InstallationDate = meter.InstallationDate;
            InitialReading = meter.InitialReading;
            VerificationDate = meter.VerificationDate;
            SelectedStatus = FindStatus(meter.StatusId);
        }

        private MeterTypeDto FindMeterType(int id)
        {
            foreach (MeterTypeDto type in MeterTypes)
            {
                if (type.Id == id)
                {
                    return type;
                }
            }

            return null;
        }

        private ConsumptionObjectDto FindObject(int id)
        {
            foreach (ConsumptionObjectDto obj in Objects)
            {
                if (obj.Id == id)
                {
                    return obj;
                }
            }

            return null;
        }

        private MeterStatusDto FindStatus(int id)
        {
            foreach (MeterStatusDto status in Statuses)
            {
                if (status.Id == id)
                {
                    return status;
                }
            }

            return null;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(SerialNumber) &&
                   SelectedMeterType != null &&
                   SelectedObject != null &&
                   SelectedStatus != null;
        }

        private void Save()
        {
            MeterDto dto = new()
            {
                Id = _meter?.Id ?? 0,
                SerialNumber = SerialNumber,
                MeterTypeId = SelectedMeterType.Id,
                ConsumptionObjectId = SelectedObject.Id,
                InstallationDate = InstallationDate,
                InitialReading = InitialReading,
                VerificationDate = VerificationDate,
                StatusId = SelectedStatus.Id
            };

            if (IsEditMode)
            {
                _meterRepository.Update(dto);
            }
            else
            {
                _meterRepository.Add(dto);
            }

            OnMeterSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnMeterSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}