using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectEditViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly StreetRepository _streetRepository;
        private readonly ObjectTypeRepository _typeRepository;
        private readonly CityRepository _cityRepository;
        private readonly RegionRepository _regionRepository;

        public event EventHandler OnObjectSaved;

        private ConsumptionObjectDto _object;
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

        // Храним задачи для синхронизации
        private Task _regionsLoadTask;
        private Task _objectTypesLoadTask;

        public ObservableCollection<RegionDto> Regions { get; set; }
        public ObservableCollection<CityDto> Cities { get; set; }
        public ObservableCollection<StreetDto> StreetsList { get; set; }
        public ObservableCollection<ObjectTypeDto> ObjectTypes { get; set; }

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
                }
            }
        }

        public StreetDto SelectedStreet
        {
            get => _selectedStreet;
            set => SetProperty(ref _selectedStreet, value);
        }

        public ObjectTypeDto SelectedObjectType
        {
            get => _selectedObjectType;
            set
            {
                if (SetProperty(ref _selectedObjectType, value))
                {
                    OnPropertyChanged(nameof(IsPrivateHouse));
                    OnPropertyChanged(nameof(IsApartmentNumberEnabled));
                    ValidateResidentCount();
                }
            }
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set => SetProperty(ref _houseNumber, value);
        }

        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set => SetProperty(ref _apartmentNumber, value);
        }

        public decimal TotalArea
        {
            get => _totalArea;
            set
            {
                SetProperty(ref _totalArea, value);
                ValidateResidentCount();
            }
        }

        public int? ResidentCount
        {
            get => _residentCount;
            set
            {
                SetProperty(ref _residentCount, value);
                ValidateResidentCount();
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
        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand AddRegionCommand { get; }
        public RelayCommand AddCityCommand { get; }
        public RelayCommand AddStreetCommand { get; }

        public ConsumptionObjectEditViewModel(ConsumptionObjectDto existingObject = null)
        {
            _objectRepository = new ConsumptionObjectRepository();
            _streetRepository = new StreetRepository();
            _typeRepository = new ObjectTypeRepository();
            _cityRepository = new CityRepository();
            _regionRepository = new RegionRepository();

            Regions = new ObservableCollection<RegionDto>();
            Cities = new ObservableCollection<CityDto>();
            StreetsList = new ObservableCollection<StreetDto>();
            ObjectTypes = new ObservableCollection<ObjectTypeDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave() && !IsLoadingData);
            CancelCommand = new RelayCommand(_ => Cancel());
            AddRegionCommand = new RelayCommand(_ => AddRegion());
            AddCityCommand = new RelayCommand(_ => AddCity());
            AddStreetCommand = new RelayCommand(_ => AddStreet());

            IsLoadingData = true;

            // ✅ СОХРАНЯЕМ ЗАДАЧИ ДЛЯ СИНХРОНИЗАЦИИ
            _regionsLoadTask = LoadRegionsAsync();
            _objectTypesLoadTask = LoadObjectTypesAsync();

            if (existingObject != null)
            {
                IsEditMode = true;
                _object = existingObject;

                // Заполняем простые поля сразу
                HouseNumber = existingObject.HouseNumber;
                ApartmentNumber = existingObject.ApartmentNumber;
                TotalArea = existingObject.TotalArea ?? 0;
                ResidentCount = existingObject.ResidentCount;

                // ✅ ЗАПУСКАЕМ ЗАГРУЗКУ ГЕО-ДАННЫХ С ОЖИДАНИЕМ
                Task.Run(async () => await LoadCityAndStreetAsync(existingObject));
            }
            else
            {
                IsEditMode = false;
                HouseNumber = string.Empty;
                ApartmentNumber = string.Empty;
                TotalArea = 0;
                ResidentCount = null;
                IsLoadingData = false;
            }
        }

        private async Task LoadRegionsAsync()
        {
            var regions = await _regionRepository.GetAllAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Regions.Clear();
                foreach (var region in regions)
                    Regions.Add(region);
            });
        }

        private async Task LoadObjectTypesAsync()
        {
            var types = await _typeRepository.GetAllAsync();
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

        private async Task LoadCitiesByRegionAsync(int regionId)
        {
            var cities = await _cityRepository.GetByRegionIdAsync(regionId);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Cities.Clear();
                foreach (var city in cities)
                    Cities.Add(city);
            });
        }

        private async Task LoadStreetsByCityAsync(int cityId)
        {
            var streets = await _streetRepository.GetByCityIdAsync(cityId);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StreetsList.Clear();
                foreach (var street in streets)
                    StreetsList.Add(street);
            });
        }

        // ✅ ИСПРАВЛЕННЫЙ МЕТОД — СИНХРОНИЗАЦИЯ ЧЕРЕЗ ДОЖИДАНИЕ ЗАДАЧ
        private async Task LoadCityAndStreetAsync(ConsumptionObjectDto obj)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== LoadCityAndStreetAsync: НАЧАЛО ===");

                // ✅ ЖДЁМ ЗАГРУЗКИ РЕГИОНОВ
                await _regionsLoadTask;
                System.Diagnostics.Debug.WriteLine($"Регионы загружены: {Regions.Count}");

                // ✅ ЖДЁМ ЗАГРУЗКИ ТИПОВ
                await _objectTypesLoadTask;
                System.Diagnostics.Debug.WriteLine($"Типы загружены: {ObjectTypes.Count}");

                // Получаем улицу
                var street = await _streetRepository.GetByIdAsync(obj.StreetId);
                if (street == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Улица с ID={obj.StreetId} не найдена");
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"✅ Улица: {street.Name}, CityId={street.CityId}");

                // Получаем город
                var city = await _cityRepository.GetByIdAsync(street.CityId);
                if (city == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Город с ID={street.CityId} не найден");
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"✅ Город: {city.Name}, RegionId={city.RegionId}");

                // Всё дальнейшее — в UI-потоке
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    // Выбираем регион
                    SelectedRegion = Regions.FirstOrDefault(r => r.Id == city.RegionId);
                    System.Diagnostics.Debug.WriteLine($"SelectedRegion: {SelectedRegion?.Name}");

                    if (SelectedRegion != null)
                    {
                        // ✅ ЖДЁМ ЗАГРУЗКИ ГОРОДОВ
                        await LoadCitiesByRegionAsync(SelectedRegion.Id);
                        System.Diagnostics.Debug.WriteLine($"Городов загружено: {Cities.Count}");

                        SelectedCity = Cities.FirstOrDefault(c => c.Id == city.Id);
                        System.Diagnostics.Debug.WriteLine($"SelectedCity: {SelectedCity?.Name}");

                        if (SelectedCity != null)
                        {
                            // ✅ ЖДЁМ ЗАГРУЗКИ УЛИЦ
                            await LoadStreetsByCityAsync(SelectedCity.Id);
                            System.Diagnostics.Debug.WriteLine($"Улиц загружено: {StreetsList.Count}");

                            SelectedStreet = StreetsList.FirstOrDefault(s => s.Id == obj.StreetId);
                            System.Diagnostics.Debug.WriteLine($"SelectedStreet: {SelectedStreet?.Name}");
                        }
                    }

                    // Выбираем тип объекта
                    SelectedObjectType = ObjectTypes.FirstOrDefault(t => t.Id == obj.ObjectTypeId);
                    System.Diagnostics.Debug.WriteLine($"SelectedObjectType: {SelectedObjectType?.Name}");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ LoadCityAndStreetAsync ERROR: {ex.Message}");
            }
            finally
            {
                IsLoadingData = false;
                System.Diagnostics.Debug.WriteLine("=== LoadCityAndStreetAsync: ЗАВЕРШЕНО ===");
            }
        }

        private void AddRegion()
        {
            var editViewModel = new RegionEditViewModel();
            var editView = new Views.Directories.RegionEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnRegionSaved += async (s, e) =>
            {
                await LoadRegionsAsync();
                var addedRegion = Regions.FirstOrDefault(r => r.Name == editViewModel.Name);
                if (addedRegion != null)
                    SelectedRegion = addedRegion;
                editView.Close();
            };
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
            var editView = new Views.Directories.CityEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnCitySaved += async (s, e) =>
            {
                await LoadCitiesByRegionAsync(SelectedRegion.Id);
                var addedCity = Cities.FirstOrDefault(c => c.Name == editViewModel.Name);
                if (addedCity != null)
                    SelectedCity = addedCity;
                editView.Close();
            };
            editView.ShowDialog();
        }

        private void AddStreet()
        {
            if (SelectedCity == null)
            {
                MessageBox.Show("Сначала выберите город", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editViewModel = new StreetEditViewModel(SelectedCity.Id);
            var editView = new Views.Directories.StreetEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnStreetSaved += async (s, e) =>
            {
                await LoadStreetsByCityAsync(SelectedCity.Id);
                var addedStreet = StreetsList.FirstOrDefault(s => s.Name == editViewModel.Name);
                if (addedStreet != null)
                    SelectedStreet = addedStreet;
                editView.Close();
            };
            editView.ShowDialog();
        }

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

        private bool CanSave()
        {
            return SelectedRegion != null &&
                   SelectedCity != null &&
                   SelectedStreet != null &&
                   SelectedObjectType != null &&
                   !string.IsNullOrWhiteSpace(HouseNumber) &&
                   string.IsNullOrEmpty(ResidentCountError);
        }

        private void Save()
        {
            ValidateResidentCount();

            if (!string.IsNullOrEmpty(ResidentCountError))
            {
                MessageBox.Show(ResidentCountError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedStreet == null)
            {
                MessageBox.Show("Выберите улицу из списка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedCity == null)
            {
                MessageBox.Show("Выберите город из списка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedRegion == null)
            {
                MessageBox.Show("Выберите регион из списка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedObjectType == null)
            {
                MessageBox.Show("Выберите тип объекта", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dto = new ConsumptionObjectDto
            {
                Id = _object?.Id ?? 0,
                StreetId = SelectedStreet.Id,
                HouseNumber = HouseNumber,
                ApartmentNumber = ApartmentNumber,
                ObjectTypeId = SelectedObjectType.Id,
                TotalArea = TotalArea,
                ResidentCount = ResidentCount
            };

            try
            {
                if (IsEditMode)
                    _objectRepository.Update(dto);
                else
                    _objectRepository.Add(dto);

                OnObjectSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}