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
        private readonly RegionRepository _regionRepository;
        private readonly CityRepository _cityRepository;
        private readonly StreetRepository _streetRepository;

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
        private bool _isLoadingData = true;

        // Для каскадного выбора нового объекта
        private bool _isChangeObjectMode;
        private RegionDto _selectedRegion;
        private CityDto _selectedCity;
        private StreetDto _selectedStreet;
        private ConsumptionObjectDto _selectedNewObject;
        private ObservableCollection<RegionDto> _regions;
        private ObservableCollection<CityDto> _cities;
        private ObservableCollection<StreetDto> _streets;
        private ObservableCollection<ConsumptionObjectDto> _allObjects;
        private ObservableCollection<ConsumptionObjectDto> _filteredObjects;

        public ObservableCollection<MeterTypeDto> MeterTypes { get; set; }
        public ObservableCollection<MeterStatusDto> Statuses { get; set; }

        public ObservableCollection<RegionDto> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public ObservableCollection<CityDto> Cities
        {
            get => _cities;
            set => SetProperty(ref _cities, value);
        }

        public ObservableCollection<StreetDto> Streets
        {
            get => _streets;
            set => SetProperty(ref _streets, value);
        }

        public ObservableCollection<ConsumptionObjectDto> FilteredObjects
        {
            get => _filteredObjects;
            set => SetProperty(ref _filteredObjects, value);
        }

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set => SetProperty(ref _isLoadingData, value);
        }

        public bool IsChangeObjectMode
        {
            get => _isChangeObjectMode;
            set
            {
                if (SetProperty(ref _isChangeObjectMode, value) && !value)
                {
                    SelectedNewObject = null;
                    SelectedRegion = null;
                    SelectedCity = null;
                    SelectedStreet = null;
                }
            }
        }

        public ConsumptionObjectDto SelectedNewObject
        {
            get => _selectedNewObject;
            set => SetProperty(ref _selectedNewObject, value);
        }

        public RegionDto SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value))
                {
                    LoadCitiesByRegion(value?.Id ?? 0);
                    SelectedCity = null;
                    SelectedStreet = null;
                    FilterObjects();
                }
            }
        }

        public CityDto SelectedCity
        {
            get => _selectedCity;
            set
            {
                if (SetProperty(ref _selectedCity, value))
                {
                    LoadStreetsByCity(value?.Id ?? 0);
                    SelectedStreet = null;
                    FilterObjects();
                }
            }
        }

        public StreetDto SelectedStreet
        {
            get => _selectedStreet;
            set
            {
                if (SetProperty(ref _selectedStreet, value))
                {
                    FilterObjects();
                }
            }
        }

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

        public MeterEditViewModel(ConsumptionObjectDto currentObject = null)
            : base(new MeterRepository(), null)
        {
            _meterTypeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();
            _regionRepository = new RegionRepository();
            _cityRepository = new CityRepository();
            _streetRepository = new StreetRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();
            Regions = new ObservableCollection<RegionDto>();
            Cities = new ObservableCollection<CityDto>();
            Streets = new ObservableCollection<StreetDto>();
            _allObjects = new ObservableCollection<ConsumptionObjectDto>();
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>();

            Title = "Регистрация счетчика";
            InstallationDate = DateTime.Today;
            LastVerificationDate = InstallationDate;
            IsChangeObjectMode = false;
            IsLoadingData = true;

            if (currentObject != null)
            {
                IsObjectReadOnly = true;
                SelectedObject = currentObject;
            }

            _ = LoadDataAsync();
        }

        public MeterEditViewModel(MeterDto existingMeter)
            : base(new MeterRepository(), existingMeter)
        {
            _meterTypeRepository = new MeterTypeRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _statusRepository = new MeterStatusRepository();
            _regionRepository = new RegionRepository();
            _cityRepository = new CityRepository();
            _streetRepository = new StreetRepository();

            MeterTypes = new ObservableCollection<MeterTypeDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();
            Regions = new ObservableCollection<RegionDto>();
            Cities = new ObservableCollection<CityDto>();
            Streets = new ObservableCollection<StreetDto>();
            _allObjects = new ObservableCollection<ConsumptionObjectDto>();
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>();

            Title = "Редактирование счетчика";
            IsObjectReadOnly = false;
            IsChangeObjectMode = false;
            IsLoadingData = true;

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

                var regions = await _regionRepository.GetAllAsync();
                Regions.Clear();
                foreach (var region in regions)
                    Regions.Add(region);

                var allObjects = await _objectRepository.GetAllAsync();
                _allObjects.Clear();
                foreach (var obj in allObjects)
                    _allObjects.Add(obj);

                if (IsEditMode && _originalItem != null)
                {
                    LoadItem(_originalItem);

                    if (_originalItem.ConsumptionObjectId > 0)
                    {
                        SelectedObject = _allObjects.FirstOrDefault(o => o.Id == _originalItem.ConsumptionObjectId);
                    }
                }

                FilterObjects();
                IsLoadingData = false;
            }, "Ошибка загрузки данных");
        }

        private async Task LoadCitiesByRegionAsync(int regionId)
        {
            if (regionId <= 0) return;
            var cities = await _cityRepository.GetByRegionIdAsync(regionId);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Cities.Clear();
                foreach (var city in cities)
                    Cities.Add(city);
            });
        }

        private void LoadCitiesByRegion(int regionId)
        {
            if (regionId <= 0) return;
            Task.Run(async () => await LoadCitiesByRegionAsync(regionId));
        }

        private async Task LoadStreetsByCityAsync(int cityId)
        {
            if (cityId <= 0) return;
            var streets = await _streetRepository.GetByCityIdAsync(cityId);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Streets.Clear();
                foreach (var street in streets)
                    Streets.Add(street);
            });
        }

        private void LoadStreetsByCity(int cityId)
        {
            if (cityId <= 0) return;
            Task.Run(async () => await LoadStreetsByCityAsync(cityId));
        }

        private void FilterObjects()
        {
            FilteredObjects.Clear();

            var filtered = _allObjects.AsEnumerable();

            if (SelectedStreet != null)
                filtered = filtered.Where(o => o.StreetId == SelectedStreet.Id);
            else if (SelectedCity != null)
                filtered = filtered.Where(o => o.CityId == SelectedCity.Id);
            else if (SelectedRegion != null)
                filtered = filtered.Where(o => o.RegionId == SelectedRegion.Id);

            foreach (var obj in filtered)
                FilteredObjects.Add(obj);
        }

        protected override void LoadItem(MeterDto item)
        {
            if (item == null) return;

            SerialNumber = item.SerialNumber;
            InstallationDate = item.InstallationDate;
            InitialReading = item.InitialReading;
            LastVerificationDate = item.LastVerificationDate;
            NextVerificationDate = item.NextVerificationDate;
            ServiceLifeYears = item.ServiceLifeYears;

            if (MeterTypes != null)
                SelectedMeterType = MeterTypes.FirstOrDefault(t => t.Id == item.MeterTypeId);

            if (Statuses != null)
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

        // ✅ Проверка с сообщением для пользователя
        private bool ValidateWithMessage()
        {
            var errors = new System.Text.StringBuilder();

            if (string.IsNullOrWhiteSpace(SerialNumber))
                errors.AppendLine("• Серийный номер не заполнен");

            if (SelectedMeterType == null)
                errors.AppendLine("• Не выбран тип счетчика");

            if (SelectedObject == null)
                errors.AppendLine("• Не выбран объект установки");

            if (SelectedStatus == null)
                errors.AppendLine("• Не выбран статус счетчика");

            if (!InstallationDate.HasValue)
                errors.AppendLine("• Не указана дата установки");
            else if (InstallationDate > DateTime.Today)
                errors.AppendLine("• Дата установки не может быть позже сегодняшнего дня");

            if (!string.IsNullOrEmpty(DateError))
                errors.AppendLine($"• {DateError}");

            if (errors.Length > 0)
            {
                MessageBox.Show($"Невозможно сохранить счетчик:\n\n{errors.ToString()}\n\nЗаполните все обязательные поля.",
                    "Ошибка валидации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        protected override async Task SaveToRepositoryAsync(MeterDto dto)
        {
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] SaveToRepositoryAsync НАЧАЛО");

            ValidateDates();

            // ✅ ПРОВЕРКА С СООБЩЕНИЕМ ПРИ НАЖАТИИ
            var errors = new System.Text.StringBuilder();

            if (string.IsNullOrWhiteSpace(SerialNumber))
                errors.AppendLine("• Серийный номер не заполнен");

            if (SelectedMeterType == null)
                errors.AppendLine("• Не выбран тип счетчика");

            if (SelectedObject == null)
                errors.AppendLine("• Не выбран объект установки");

            if (SelectedStatus == null)
                errors.AppendLine("• Не выбран статус счетчика");

            if (!InstallationDate.HasValue)
                errors.AppendLine("• Не указана дата установки");
            else if (InstallationDate > DateTime.Today)
                errors.AppendLine("• Дата установки не может быть позже сегодняшнего дня");

            if (!string.IsNullOrEmpty(DateError))
                errors.AppendLine($"• {DateError}");

            if (errors.Length > 0)
            {
                MessageBox.Show($"Невозможно сохранить счетчик:\n\n{errors.ToString()}\n\nЗаполните все обязательные поля.",
                    "Ошибка валидации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // ✅ ПОДТВЕРЖДЕНИЕ СОХРАНЕНИЯ (для редактирования)
            if (IsEditMode)
            {
                var confirmResult = MessageBox.Show(
                    $"Сохранить изменения для счетчика \"{SerialNumber}\"?",
                    "Подтверждение сохранения",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult != MessageBoxResult.Yes)
                    return;
            }

            if (IsChangeObjectMode && SelectedNewObject != null)
            {
                dto.ConsumptionObjectId = SelectedNewObject.Id;
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Объект изменён на ID={SelectedNewObject.Id}");
            }

            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Вызов _repository.UpdateAsync...");

            try
            {
                if (IsEditMode)
                    await _repository.UpdateAsync(dto);
                else
                    await _repository.AddAsync(dto);

                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Сохранение УСПЕШНО");

                // ✅ СООБЩЕНИЕ ОБ УСПЕХЕ
                MessageBox.Show($"Счетчик \"{SerialNumber}\" успешно сохранен!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                IsLoadingData = false;
                RaiseOnSaved();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ОШИБКА: {ex.Message}");
                IsLoadingData = false;
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override bool CanSave()
        {
            bool result = !string.IsNullOrWhiteSpace(SerialNumber) &&
                           SelectedMeterType != null &&
                           SelectedObject != null &&
                           SelectedStatus != null &&
                           InstallationDate.HasValue &&
                           string.IsNullOrEmpty(DateError);

            // Для отладки
            System.Diagnostics.Debug.WriteLine($"CanSave = {result}");

            return result;
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
        }

        private void ValidateDates()
        {
            DateError = string.Empty;

            if (InstallationDate > DateTime.Today)
                DateError = "Дата установки не может быть позже сегодняшнего дня";
            else if (LastVerificationDate.HasValue && LastVerificationDate > DateTime.Today)
                DateError = "Дата последней поверки не может быть позже сегодняшнего дня";
            else if (LastVerificationDate.HasValue && NextVerificationDate.HasValue && NextVerificationDate <= LastVerificationDate)
                DateError = "Дата следующей поверки должна быть позже даты последней поверки";
            else if (RemovalDate.HasValue && NextVerificationDate.HasValue && NextVerificationDate > RemovalDate)
                DateError = "Дата следующей поверки не может быть позже даты изъятия счетчика";
        }
    }
}