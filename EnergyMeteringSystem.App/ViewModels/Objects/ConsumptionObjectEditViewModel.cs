using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectEditViewModel : EditViewModelBase<ConsumptionObjectDto, ConsumptionObjectRepository>
    {
        private readonly StreetRepository _streetRepository;
        private readonly ObjectTypeRepository _typeRepository;
        private readonly CityRepository _cityRepository;
        private readonly RegionRepository _regionRepository;
        private readonly EnergyMeteringSystemEntities _context;

        // Поля
        private StreetDto _selectedStreet;
        private ObjectTypeDto _selectedObjectType;
        private string _houseNumber;
        private string _apartmentNumber;
        private decimal _totalArea;
        private int? _residentCount;
        private string _residentCountError;
        private CityDto _selectedCity;
        private RegionDto _selectedRegion;
        private bool _isLoadingData = true;

        // Флаги изменений
        private bool _isStreetChanged;
        private bool _isHouseNumberChanged;
        private bool _isApartmentNumberChanged;
        private bool _isObjectTypeChanged;
        private bool _isTotalAreaChanged;
        private bool _isResidentCountChanged;

        // Кэш для CanSave()
        private bool _cachedCanSaveResult;
        private string _cachedCanSaveReason;
        private bool _isCanSaveDirty = true;

        // Коллекции
        public ObservableCollection<RegionDto> Regions { get; set; }
        public ObservableCollection<CityDto> Cities { get; set; }
        public ObservableCollection<StreetDto> StreetsList { get; set; }
        public ObservableCollection<ObjectTypeDto> ObjectTypes { get; set; }

        // Свойства
        public bool IsLoadingData
        {
            get => _isLoadingData;
            set => SetProperty(ref _isLoadingData, value);
        }

        public RegionDto SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value) && value != null)
                {
                    _ = LoadCitiesByRegionAsync(value.Id);
                    _isCanSaveDirty = true;
                }
            }
        }

        public CityDto SelectedCity
        {
            get => _selectedCity;
            set
            {
                if (SetProperty(ref _selectedCity, value) && value != null)
                {
                    _ = LoadStreetsByCityAsync(value.Id);
                    _isCanSaveDirty = true;
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
                    _isStreetChanged = true;
                    _isCanSaveDirty = true;
                }
            }
        }

        public ObjectTypeDto SelectedObjectType
        {
            get => _selectedObjectType;
            set
            {
                if (SetProperty(ref _selectedObjectType, value))
                {
                    _isObjectTypeChanged = true;
                    _isCanSaveDirty = true;
                    OnPropertyChanged(nameof(IsPrivateHouse));
                    OnPropertyChanged(nameof(IsApartmentNumberEnabled));
                    ValidateResidentCount();
                }
            }
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set
            {
                if (SetProperty(ref _houseNumber, value))
                {
                    _isHouseNumberChanged = true;
                    _isCanSaveDirty = true;
                }
            }
        }

        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set
            {
                if (SetProperty(ref _apartmentNumber, value))
                {
                    _isApartmentNumberChanged = true;
                }
            }
        }

        public decimal TotalArea
        {
            get => _totalArea;
            set
            {
                if (SetProperty(ref _totalArea, value))
                {
                    _isTotalAreaChanged = true;
                    ValidateResidentCount();
                }
            }
        }

        public int? ResidentCount
        {
            get => _residentCount;
            set
            {
                if (SetProperty(ref _residentCount, value))
                {
                    _isResidentCountChanged = true;
                    ValidateResidentCount();
                    _isCanSaveDirty = true;
                }
            }
        }

        public string ResidentCountError
        {
            get => _residentCountError;
            set => SetProperty(ref _residentCountError, value);
        }

        public bool HasResidentCountError => !string.IsNullOrEmpty(ResidentCountError);
        public bool IsPrivateHouse => SelectedObjectType?.Name == "Частный дом";
        public bool IsApartmentNumberEnabled => !IsPrivateHouse;

        // Команды
        public RelayCommand AddRegionCommand { get; }
        public RelayCommand AddCityCommand { get; }
        public RelayCommand AddStreetCommand { get; }

        // ============================================================
        // КОНСТРУКТОРЫ
        // ============================================================

        // Конструктор для добавления
        public ConsumptionObjectEditViewModel()
            : base(new ConsumptionObjectRepository(), null)
        {
            _streetRepository = new StreetRepository();
            _typeRepository = new ObjectTypeRepository();
            _cityRepository = new CityRepository();
            _regionRepository = new RegionRepository();
            _context = new EnergyMeteringSystemEntities();

            Regions = new ObservableCollection<RegionDto>();
            Cities = new ObservableCollection<CityDto>();
            StreetsList = new ObservableCollection<StreetDto>();
            ObjectTypes = new ObservableCollection<ObjectTypeDto>();

            AddRegionCommand = new RelayCommand(_ => AddRegion());
            AddCityCommand = new RelayCommand(_ => AddCity());
            AddStreetCommand = new RelayCommand(_ => AddStreet());

            Title = "Добавление объекта";
            IsEditMode = false;

            HouseNumber = string.Empty;
            ApartmentNumber = string.Empty;
            TotalArea = 0;
            ResidentCount = null;

            _ = LoadDataAsync();
        }

        // Конструктор для редактирования
        public ConsumptionObjectEditViewModel(ConsumptionObjectDto existingObject)
            : base(new ConsumptionObjectRepository(), existingObject)
        {
            _streetRepository = new StreetRepository();
            _typeRepository = new ObjectTypeRepository();
            _cityRepository = new CityRepository();
            _regionRepository = new RegionRepository();
            _context = new EnergyMeteringSystemEntities();

            Regions = new ObservableCollection<RegionDto>();
            Cities = new ObservableCollection<CityDto>();
            StreetsList = new ObservableCollection<StreetDto>();
            ObjectTypes = new ObservableCollection<ObjectTypeDto>();

            Title = "Редактирование объекта";
            IsEditMode = true;

            AddRegionCommand = new RelayCommand(_ => AddRegion());
            AddCityCommand = new RelayCommand(_ => AddCity());
            AddStreetCommand = new RelayCommand(_ => AddStreet());

            _originalItem = existingObject;
            HouseNumber = existingObject.HouseNumber;
            ApartmentNumber = existingObject.ApartmentNumber;
            TotalArea = existingObject.TotalArea ?? 0;
            ResidentCount = existingObject.ResidentCount;

            ResetChangeFlags();

            _ = LoadAllDataAsync(existingObject);
        }

        // ============================================================
        // ЗАГРУЗКА ДАННЫХ
        // ============================================================

        private void ResetChangeFlags()
        {
            _isStreetChanged = false;
            _isHouseNumberChanged = false;
            _isApartmentNumberChanged = false;
            _isObjectTypeChanged = false;
            _isTotalAreaChanged = false;
            _isResidentCountChanged = false;
            _isCanSaveDirty = true;
        }

        private async Task LoadRegionsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadRegionsAsync: START");
                var regions = await _regionRepository.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadRegionsAsync: loaded {regions.Count} regions");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Regions.Clear();
                    foreach (var region in regions)
                        Regions.Add(region);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRegionsAsync ERROR: {ex.Message}");
            }
        }

        private async Task LoadObjectTypesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadObjectTypesAsync: START");
                var types = await _typeRepository.GetAllAsync();
                System.Diagnostics.Debug.WriteLine($"LoadObjectTypesAsync: loaded {types.Count} types");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ObjectTypes.Clear();
                    foreach (var type in types)
                    {
                        ObjectTypes.Add(new ObjectTypeDto
                        {
                            Id = type.Id,
                            Name = type.Name,
                            Description = type.Description
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadObjectTypesAsync ERROR: {ex.Message}");
            }
        }

        private async Task LoadCitiesByRegionAsync(int regionId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadCitiesByRegionAsync: regionId={regionId}");
                var cities = await _cityRepository.GetByRegionIdAsync(regionId);
                System.Diagnostics.Debug.WriteLine($"LoadCitiesByRegionAsync: loaded {cities.Count} cities");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Cities.Clear();
                    foreach (var city in cities)
                        Cities.Add(city);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCitiesByRegionAsync ERROR: {ex.Message}");
            }
        }

        private async Task LoadStreetsByCityAsync(int cityId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"LoadStreetsByCityAsync: cityId={cityId}");
                var streets = await _streetRepository.GetByCityIdAsync(cityId);
                System.Diagnostics.Debug.WriteLine($"LoadStreetsByCityAsync: loaded {streets.Count} streets");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StreetsList.Clear();
                    foreach (var street in streets)
                        StreetsList.Add(street);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadStreetsByCityAsync ERROR: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            await LoadRegionsAsync();
            await LoadObjectTypesAsync();
            IsLoadingData = false;
            _isCanSaveDirty = true;
            RaiseCanExecuteChanged();
        }

        // ✅ ВСПОМОГАТЕЛЬНЫЙ КЛАСС ДЛЯ ДАННЫХ АДРЕСА
        private class AddressData
        {
            public int StreetId { get; set; }
            public string StreetName { get; set; }
            public int CityId { get; set; }
            public string CityName { get; set; }
            public int RegionId { get; set; }
            public string RegionName { get; set; }
        }

        // ✅ ПОЛУЧЕНИЕ ДАННЫХ АДРЕСА (ЧЕРЕЗ РЕПОЗИТОРИИ)
        private async Task<AddressData> GetAddressDataAsync(int streetId)
        {
            try
            {
                // Загружаем улицу через репозиторий
                var street = await _streetRepository.GetByIdAsync(streetId);
                if (street == null) return null;

                // Загружаем город
                var city = await _cityRepository.GetByIdAsync(street.CityId);
                if (city == null) return null;

                // Загружаем регион
                var region = await _regionRepository.GetByIdAsync(city.RegionId);
                if (region == null) return null;

                return new AddressData
                {
                    StreetId = street.Id,
                    StreetName = street.Name,
                    CityId = city.Id,
                    CityName = city.Name,
                    RegionId = region.Id,
                    RegionName = region.Name
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAddressDataAsync ERROR: {ex.Message}");
                return null;
            }
        }

        // ✅ ПОСЛЕДОВАТЕЛЬНАЯ ЗАГРУЗКА ВСЕХ ДАННЫХ
        // ✅ ПОСЛЕДОВАТЕЛЬНАЯ ЗАГРУЗКА ВСЕХ ДАННЫХ
        private async Task LoadAllDataAsync(ConsumptionObjectDto obj)
        {
            try
            {
                IsLoadingData = true;

                // 1. Загружаем регионы
                await LoadRegionsAsync();

                // 2. Загружаем типы объектов
                await LoadObjectTypesAsync();

                // 3. Получаем данные адреса
                var addressData = await GetAddressDataAsync(obj.StreetId);

                if (addressData == null)
                {
                    System.Diagnostics.Debug.WriteLine("AddressData is null!");
                    IsLoadingData = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"AddressData: Street={addressData.StreetName}, City={addressData.CityName}, Region={addressData.RegionName}");

                // 4. Устанавливаем значения в UI (синхронно!)
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    // Выбираем регион
                    SelectedRegion = Regions.FirstOrDefault(r => r.Id == addressData.RegionId);
                    System.Diagnostics.Debug.WriteLine($"SelectedRegion: {SelectedRegion?.Name ?? "null"}");

                    if (SelectedRegion != null)
                    {
                        // ✅ ЖДЕМ загрузку городов!
                        await LoadCitiesByRegionAsync(SelectedRegion.Id);

                        // Выбираем город
                        SelectedCity = Cities.FirstOrDefault(c => c.Id == addressData.CityId);
                        System.Diagnostics.Debug.WriteLine($"SelectedCity: {SelectedCity?.Name ?? "null"}");

                        if (SelectedCity != null)
                        {
                            // ✅ ЖДЕМ загрузку улиц!
                            await LoadStreetsByCityAsync(SelectedCity.Id);

                            // Выбираем улицу
                            SelectedStreet = StreetsList.FirstOrDefault(s => s.Id == addressData.StreetId);
                            System.Diagnostics.Debug.WriteLine($"SelectedStreet: {SelectedStreet?.Name ?? "null"}");
                        }
                    }

                    // Выбираем тип объекта
                    SelectedObjectType = ObjectTypes.FirstOrDefault(t => t.Id == obj.ObjectTypeId);
                    System.Diagnostics.Debug.WriteLine($"SelectedObjectType: {SelectedObjectType?.Name ?? "null"}");

                    // Сбрасываем флаги изменений
                    ResetChangeFlags();

                    // ✅ ПРИНУДИТЕЛЬНО ОБНОВЛЯЕМ CanSave()
                    _isCanSaveDirty = true;
                    CanSave();

                    IsLoadingData = false;
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadAllDataAsync ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                IsLoadingData = false;
            }
        }

        // ============================================================
        // ДОБАВЛЕНИЕ СПРАВОЧНИКОВ
        // ============================================================

        private void AddRegion()
        {
            var editViewModel = new RegionEditViewModel();
            var editView = new Views.Directories.RegionEditView { DataContext = editViewModel };

            editViewModel.OnRegionSaved += async (s, e) =>
            {
                await LoadRegionsAsync();
                var addedRegion = Regions.FirstOrDefault(r => r.Name == editViewModel.Name);
                if (addedRegion != null)
                    SelectedRegion = addedRegion;
                editView.Close();
            };
            editViewModel.OnCancelled += (s, e) => editView.Close();
            editView.ShowDialog();
        }

        private void AddCity()
        {
            if (SelectedRegion == null)
            {
                MessageBox.Show("Сначала выберите регион", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editViewModel = new CityEditViewModel(SelectedRegion.Id);
            var editView = new Views.Directories.CityEditView { DataContext = editViewModel };
            var regionId = SelectedRegion.Id;

            editViewModel.OnCitySaved += async (s, e) =>
            {
                await LoadCitiesByRegionAsync(regionId);
                var addedCity = Cities.FirstOrDefault(c => c.Name == editViewModel.Name);
                if (addedCity != null)
                    SelectedCity = addedCity;
                editView.Close();
            };
            editViewModel.OnCancelled += (s, e) => editView.Close();
            editView.ShowDialog();
        }

        private void AddStreet()
        {
            if (SelectedCity == null)
            {
                MessageBox.Show("Сначала выберите город", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editViewModel = new StreetEditViewModel(SelectedCity.Id, SelectedCity.Name);
            var editView = new Views.Directories.StreetEditView { DataContext = editViewModel };
            var cityId = SelectedCity.Id;

            editViewModel.OnStreetSaved += async (s, e) =>
            {
                await LoadStreetsByCityAsync(cityId);
                var addedStreet = StreetsList.FirstOrDefault(st => st.Name == editViewModel.Name);
                if (addedStreet != null)
                    SelectedStreet = addedStreet;
                editView.Close();
            };
            editViewModel.OnCancelled += (s, e) => editView.Close();
            editView.ShowDialog();
        }

        // ============================================================
        // ВАЛИДАЦИЯ
        // ============================================================

        private void ValidateResidentCount()
        {
            ResidentCountError = string.Empty;

            if (!ResidentCount.HasValue)
            {
                if (SelectedObjectType?.Name != "Магазин")
                    ResidentCountError = "Укажите количество проживающих";
                return;
            }

            if (ResidentCount.Value <= 0)
            {
                ResidentCountError = "Количество проживающих должно быть больше 0";
                return;
            }

            if (TotalArea > 0)
            {
                int maxResidents = CalculateMaxResidents(TotalArea);
                if (ResidentCount.Value > maxResidents)
                {
                    string normText = IsPrivateHouse ? "18 м²" : "12 м²";
                    ResidentCountError = $"Согласно санитарным нормам, требуется не менее {normText} на человека.\n" +
                                         $"Ваша площадь: {TotalArea:F1} м². Максимум жильцов: {maxResidents}.\n" +
                                         $"Укажите меньше жильцов или увеличьте площадь.";
                }
            }
        }

        private int CalculateMaxResidents(decimal totalArea)
        {
            decimal normPerPerson = IsPrivateHouse ? 18m : 12m;
            return Math.Max(1, (int)Math.Floor(totalArea / normPerPerson));
        }

        // ============================================================
        // ОПТИМИЗИРОВАННЫЙ CanSave() С КЭШИРОВАНИЕМ
        // ============================================================

        protected override bool CanSave()
        {
            // Возвращаем кэшированный результат, если ничего не менялось
            if (!_isCanSaveDirty)
                return _cachedCanSaveResult;

            // Быстрая проверка без сложных вычислений
            if (SelectedRegion == null)
            {
                _cachedCanSaveReason = "Выберите регион";
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            if (SelectedCity == null)
            {
                _cachedCanSaveReason = "Выберите город";
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            if (SelectedStreet == null)
            {
                _cachedCanSaveReason = "Выберите улицу";
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            if (SelectedObjectType == null)
            {
                _cachedCanSaveReason = "Выберите тип объекта";
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            if (string.IsNullOrWhiteSpace(HouseNumber))
            {
                _cachedCanSaveReason = "Введите номер дома";
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            // Проверка ResidentCount только если он заполнен
            if (ResidentCount.HasValue && ResidentCount.Value <= 0)
            {
                _cachedCanSaveReason = "Количество жильцов должно быть > 0";
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            // Проверка ошибки ResidentCount
            if (!string.IsNullOrEmpty(ResidentCountError))
            {
                _cachedCanSaveReason = ResidentCountError;
                _cachedCanSaveResult = false;
                _isCanSaveDirty = false;
                return false;
            }

            _cachedCanSaveReason = null;
            _cachedCanSaveResult = true;
            _isCanSaveDirty = false;
            return true;
        }

        // ============================================================
        // СРАВНЕНИЕ ДАННЫХ И ОПРЕДЕЛЕНИЕ ИЗМЕНЕНИЙ
        // ============================================================

        /// <summary>
        /// Возвращает список полей, которые реально изменились
        /// </summary>
        private (bool HasChanges, List<string> ChangedFields) GetChangedFields()
        {
            var changedFields = new List<string>();

            if (_isStreetChanged) changedFields.Add(nameof(ConsumptionObjectDto.StreetId));
            if (_isHouseNumberChanged) changedFields.Add(nameof(ConsumptionObjectDto.HouseNumber));
            if (_isApartmentNumberChanged) changedFields.Add(nameof(ConsumptionObjectDto.ApartmentNumber));
            if (_isObjectTypeChanged) changedFields.Add(nameof(ConsumptionObjectDto.ObjectTypeId));
            if (_isTotalAreaChanged) changedFields.Add(nameof(ConsumptionObjectDto.TotalArea));
            if (_isResidentCountChanged) changedFields.Add(nameof(ConsumptionObjectDto.ResidentCount));

            return (changedFields.Any(), changedFields);
        }

        /// <summary>
        /// Проверяет, есть ли реальные изменения в данных
        /// </summary>
        private bool HasRealChanges()
        {
            if (_originalItem == null) return true; // Новый объект

            var current = GetDto();

            // Сравниваем все значимые поля
            if (current.StreetId != _originalItem.StreetId) return true;
            if (!string.Equals(current.HouseNumber?.Trim(), _originalItem.HouseNumber?.Trim(), StringComparison.Ordinal)) return true;
            if (!string.Equals(current.ApartmentNumber?.Trim(), _originalItem.ApartmentNumber?.Trim(), StringComparison.Ordinal)) return true;
            if (current.ObjectTypeId != _originalItem.ObjectTypeId) return true;
            if (current.TotalArea != _originalItem.TotalArea) return true;
            if (current.ResidentCount != _originalItem.ResidentCount) return true;

            return false;
        }

        // ============================================================
        // ОСНОВНЫЕ МЕТОДЫ
        // ============================================================

        protected override void LoadItem(ConsumptionObjectDto item)
        {
            // Загрузка выполняется в конструкторе
        }

        protected override ConsumptionObjectDto GetDto()
        {
            return new ConsumptionObjectDto
            {
                Id = _originalItem?.Id ?? 0,
                StreetId = SelectedStreet?.Id ?? 0,
                HouseNumber = HouseNumber,
                ApartmentNumber = ApartmentNumber,
                ObjectTypeId = SelectedObjectType?.Id ?? 0,
                TotalArea = TotalArea,
                ResidentCount = ResidentCount
            };
        }

        // ✅ ОПТИМИЗИРОВАННОЕ СОХРАНЕНИЕ
        protected override async Task<bool> SaveToRepositoryAsync(ConsumptionObjectDto dto)
        {
            // Проверка: есть ли реальные изменения?
            if (IsEditMode && !HasRealChanges())
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("Нет изменений для сохранения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });
                return false;
            }

            var (hasChanges, changedFields) = GetChangedFields();
            if (hasChanges)
            {
                System.Diagnostics.Debug.WriteLine($"Сохранение изменений: {string.Join(", ", changedFields)}");
            }

            if (IsEditMode)
            {
                var updated = await _repository.UpdateAsync(dto);
                if (!updated)
                    throw new InvalidOperationException("Не удалось обновить объект в базе данных");
            }
            else
            {
                await _repository.AddAsync(dto);
            }

            ResetChangeFlags();
            return true;
        }

        protected override string GetSaveValidationMessage()
        {
            if (!string.IsNullOrEmpty(_cachedCanSaveReason))
                return _cachedCanSaveReason;
            if (!string.IsNullOrEmpty(ResidentCountError))
                return ResidentCountError;
            return base.GetSaveValidationMessage();
        }

        // ============================================================
        // ПЕРЕОПРЕДЕЛЕНИЕ ДЛЯ ОБНОВЛЕНИЯ CanSave()
        // ============================================================

        protected override bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            HasChanges = true;
            _isCanSaveDirty = true;
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            return true;
        }

        // ============================================================
        // IDISPOSABLE
        // ============================================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}