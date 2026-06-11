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

        public ObservableCollection<RegionDto> Regions { get; set; }
        public ObservableCollection<CityDto> Cities { get; set; }
        public ObservableCollection<StreetDto> StreetsList { get; set; }
        public ObservableCollection<ObjectTypeDto> ObjectTypes { get; set; }

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
            System.Diagnostics.Debug.WriteLine($"ConsumptionObjectEditViewModel конструктор START, existingObject={existingObject != null}");

            _objectRepository = new ConsumptionObjectRepository();
            _streetRepository = new StreetRepository();
            _typeRepository = new ObjectTypeRepository();
            _cityRepository = new CityRepository();
            _regionRepository = new RegionRepository();

            Regions = new ObservableCollection<RegionDto>();
            Cities = new ObservableCollection<CityDto>();
            StreetsList = new ObservableCollection<StreetDto>();
            ObjectTypes = new ObservableCollection<ObjectTypeDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
            AddRegionCommand = new RelayCommand(_ => AddRegion());
            AddCityCommand = new RelayCommand(_ => AddCity());
            AddStreetCommand = new RelayCommand(_ => AddStreet());

            Task.Run(async () => await LoadRegionsAsync());
            Task.Run(async () => await LoadObjectTypesAsync());

            if (existingObject != null)
            {
                IsEditMode = true;
                System.Diagnostics.Debug.WriteLine($"Загружаем объект для редактирования: ID={existingObject.Id}");
                Task.Run(async () => await LoadObjectAsync(existingObject));
            }
            else
            {
                IsEditMode = false;
                System.Diagnostics.Debug.WriteLine("Режим добавления нового объекта");
                HouseNumber = string.Empty;
                ApartmentNumber = string.Empty;
                TotalArea = 0;
                ResidentCount = null;
            }

            System.Diagnostics.Debug.WriteLine("ConsumptionObjectEditViewModel конструктор END");
        }

        private async Task LoadRegionsAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                var regions = await _regionRepository.GetAllAsync();
                Regions.Clear();
                foreach (var region in regions)
                    Regions.Add(region);
            });
        }

        private async Task LoadObjectTypesAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("LoadObjectTypesAsync START");

                    var types = await _typeRepository.GetAllAsync();

                    System.Diagnostics.Debug.WriteLine($"LoadObjectTypesAsync: получено {types.Count} типов");

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

                    System.Diagnostics.Debug.WriteLine("LoadObjectTypesAsync END");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LoadObjectTypesAsync ERROR: {ex.Message}");
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

        private async Task LoadObjectAsync(ConsumptionObjectDto obj)
        {
            System.Diagnostics.Debug.WriteLine($"LoadObjectAsync START для объекта ID={obj?.Id}");

            if (obj == null)
            {
                System.Diagnostics.Debug.WriteLine("LoadObjectAsync: obj == null");
                return;
            }

            try
            {
                _object = obj;

                // 1. Загружаем регионы (асинхронно, с ожиданием)
                if (Regions.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Загружаем регионы...");
                    await LoadRegionsAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено регионов: {Regions.Count}");
                }

                // 2. Загружаем типы объектов (асинхронно, с ожиданием)
                if (ObjectTypes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Загружаем типы объектов...");
                    await LoadObjectTypesAsync();
                    System.Diagnostics.Debug.WriteLine($"Загружено типов: {ObjectTypes.Count}");
                }

                // 3. Получаем улицу из БД
                System.Diagnostics.Debug.WriteLine($"Получаем улицу с ID={obj.StreetId}");
                var street = await _streetRepository.GetByIdAsync(obj.StreetId);
                if (street == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ОШИБКА: улица с ID={obj.StreetId} не найдена");
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"Найдена улица: ID={street.Id}, Name='{street.Name}', CityId={street.CityId}");

                // 4. Получаем город из БД
                System.Diagnostics.Debug.WriteLine($"Получаем город с ID={street.CityId}");
                var city = await _cityRepository.GetByIdAsync(street.CityId);
                if (city == null)
                {
                    System.Diagnostics.Debug.WriteLine($"ОШИБКА: город с ID={street.CityId} не найден");
                    return;
                }
                System.Diagnostics.Debug.WriteLine($"Найден город: ID={city.Id}, Name='{city.Name}', RegionId={city.RegionId}");

                // 5. Выбираем регион (синхронно, данные уже в памяти)
                SelectedRegion = Regions.FirstOrDefault(r => r.Id == city.RegionId);
                System.Diagnostics.Debug.WriteLine($"SelectedRegion = {SelectedRegion?.Name ?? "null"}");

                if (SelectedRegion != null)
                {
                    // 6. Загружаем города выбранного региона (асинхронно, с ожиданием)
                    System.Diagnostics.Debug.WriteLine($"Загружаем города для региона {SelectedRegion.Name}...");
                    await LoadCitiesByRegionAsync(SelectedRegion.Id);
                    System.Diagnostics.Debug.WriteLine($"Загружено городов: {Cities.Count}");

                    // 7. Выбираем город (синхронно, данные уже в памяти)
                    SelectedCity = Cities.FirstOrDefault(c => c.Id == city.Id);
                    System.Diagnostics.Debug.WriteLine($"SelectedCity = {SelectedCity?.Name ?? "null"}");

                    if (SelectedCity != null)
                    {
                        // 8. Загружаем улицы выбранного города (асинхронно, с ожиданием)
                        System.Diagnostics.Debug.WriteLine($"Загружаем улицы для города {SelectedCity.Name}...");
                        await LoadStreetsByCityAsync(SelectedCity.Id);
                        System.Diagnostics.Debug.WriteLine($"Загружено улиц: {StreetsList.Count}");

                        // 9. Выбираем улицу (синхронно, данные уже в памяти)
                        SelectedStreet = StreetsList.FirstOrDefault(s => s.Id == obj.StreetId);
                        System.Diagnostics.Debug.WriteLine($"SelectedStreet = {SelectedStreet?.Name ?? "null"}");
                    }
                }

                // 10. Выбираем тип объекта (синхронно)
                SelectedObjectType = ObjectTypes.FirstOrDefault(t => t.Id == obj.ObjectTypeId);
                System.Diagnostics.Debug.WriteLine($"SelectedObjectType = {SelectedObjectType?.Name ?? "null"}");

                // 11. Заполняем остальные поля
                HouseNumber = obj.HouseNumber;
                ApartmentNumber = obj.ApartmentNumber;
                TotalArea = obj.TotalArea ?? 0;
                ResidentCount = obj.ResidentCount;

                System.Diagnostics.Debug.WriteLine("LoadObjectAsync: ОБЪЕКТ УСПЕШНО ЗАГРУЖЕН");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadObjectAsync: ОШИБКА - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка при загрузке объекта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
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

        private async void Save()
        {
            ValidateResidentCount();

            if (!string.IsNullOrEmpty(ResidentCountError))
            {
                MessageBox.Show(ResidentCountError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    await _objectRepository.UpdateAsync(dto);
                else
                    await _objectRepository.AddAsync(dto);

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