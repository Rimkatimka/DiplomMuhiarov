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
        public DateTime? VerificationDate { get; set; }

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
            _meterRepository = new MeterRepository();
            _typeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            InstallationDate = DateTime.Today;

            LoadData();

            if (existingMeter != null)
            {
                IsEditMode = true;
                LoadMeter(existingMeter);
            }
        }

        private void LoadData()
        {
            // Загрузка типов счетчиков
            var types = _typeRepository.GetAll();
            foreach (var type in types)
                MeterTypes.Add(type);

            // Загрузка объектов
            var objects = _objectRepository.GetAll();
            foreach (var obj in objects)
                Objects.Add(obj);

            // Загрузка статусов - ПРЕОБРАЗУЕМ DirectoryDto в MeterStatusDto
            var statuses = _statusRepository.GetAll(); // это List<DirectoryDto>
            foreach (var item in statuses)
            {
                Statuses.Add(new MeterStatusDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    CanAcceptReadings = true // значение по умолчанию
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
            foreach (var type in MeterTypes)
                if (type.Id == id) return type;
            return null;
        }

        private ConsumptionObjectDto FindObject(int id)
        {
            foreach (var obj in Objects)
                if (obj.Id == id) return obj;
            return null;
        }

        private MeterStatusDto FindStatus(int id)
        {
            foreach (var status in Statuses)
                if (status.Id == id) return status;
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
            var dto = new MeterDto
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