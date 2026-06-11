using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectEditViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly StreetRepository _streetRepository;
        private readonly ObjectTypeRepository _typeRepository;
        private readonly CityRepository _cityRepository;
        private readonly RegionRepository _regionRepository;

        public event EventHandler OnSaved;
        private ConsumptionObjectDto _object;
        private StreetDto _selectedStreet;
        private ObjectTypeDto _selectedObjectType;
        private string _houseNumber;
        private string _apartmentNumber;
        private decimal _totalArea;
        private int? _residentCount;
        private string _residentCountError;
        private CityDto _selectedCity;
        private ObservableCollection<CityDto> _cities;
        private ObservableCollection<StreetDto> _streets;

        public event EventHandler OnObjectSaved;

        public ObservableCollection<StreetDto> Streets { get; set; }
        public ObservableCollection<ObjectTypeDto> ObjectTypes { get; set; }

        public ObservableCollection<CityDto> Cities
        {
            get => _cities;
            set => SetProperty(ref _cities, value);
        }

        public ObservableCollection<StreetDto> StreetsList
        {
            get => _streets;
            set => SetProperty(ref _streets, value);
        }

        public bool IsApartmentNumberEnabled => !IsPrivateHouse;

        public AsyncRelayCommand AddCityCommand { get; }
        public AsyncRelayCommand AddStreetCommand { get; }

        private RegionDto _selectedRegion;
        private ObservableCollection<RegionDto> _regions;

        public ObservableCollection<RegionDto> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public RegionDto SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value))
                {
                    if (value != null)
                    {
                        _ = LoadCitiesByRegionAsync(value.Id);
                    }
                    else
                    {
                        Cities?.Clear();
                        StreetsList?.Clear();
                        SelectedCity = null;
                        SelectedStreet = null;
                    }
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
                    if (value != null)
                    {
                        _ = LoadStreetsByCityAsync(value.Id);
                    }
                    else
                    {
                        StreetsList?.Clear();
                        SelectedStreet = null;
                    }
                }
            }
        }

        public StreetDto SelectedStreet
        {
            get => _selectedStreet;
            set => SetProperty(ref _selectedStreet, value);
        }

        public bool IsPrivateHouse
        {
            get => SelectedObjectType?.Name == "Частный дом";
        }

        public int? ResidentCount
        {
            get => _residentCount;
            set
            {
                if (SetProperty(ref _residentCount, value))
                {
                    ValidateResidentCount();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ResidentCountError
        {
            get => _residentCountError;
            set => SetProperty(ref _residentCountError, value);
        }

        public bool HasResidentCountError => !string.IsNullOrEmpty(ResidentCountError);

        public ObjectTypeDto SelectedObjectType
        {
            get => _selectedObjectType;
            set
            {
                if (SetProperty(ref _selectedObjectType, value))
                {
                    OnPropertyChanged(nameof(IsPrivateHouse));
                    OnPropertyChanged(nameof(IsApartmentNumberEnabled));

                    if (IsPrivateHouse)
                    {
                        ApartmentNumber = string.Empty;
                        OnPropertyChanged(nameof(ApartmentNumber));
                    }

                    ValidateResidentCount();
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set
            {
                SetProperty(ref _houseNumber, value);
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
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
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public AsyncRelayCommand AddRegionCommand { get; }
        public bool IsEditMode { get; private set; }

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

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
            Streets = new ObservableCollection<StreetDto>();
            ObjectTypes = new ObservableCollection<ObjectTypeDto>();

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
            AddRegionCommand = new AsyncRelayCommand(async () => await AddRegionAsync());
            AddCityCommand = new AsyncRelayCommand(async () => await AddCityAsync());
            AddStreetCommand = new AsyncRelayCommand(async () => await AddStreetAsync());

            _ = LoadInitialDataAsync();

            if (existingObject != null)
            {
                IsEditMode = true;
                _object = existingObject;
                _ = LoadObjectAsync(existingObject);
            }
        }

        private async Task LoadInitialDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                await LoadRegionsAsync();
                await LoadObjectTypesAsync();
            }, "Ошибка загрузки данных");
        }

        private async Task LoadRegionsAsync()
        {
            var regions = await _regionRepository.GetAllAsync();
            Regions.Clear();
            foreach (var region in regions)
                Regions.Add(region);
        }

        private async Task LoadObjectTypesAsync()
        {
            var types = await _typeRepository.GetAllAsync();
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
        }

        private async Task LoadCitiesByRegionAsync(int regionId)
        {
            var cities = await _cityRepository.GetByRegionIdAsync(regionId);
            Cities.Clear();
            foreach (var city in cities)
                Cities.Add(city);

            SelectedCity = null;
        }

        private async Task LoadStreetsByCityAsync(int cityId)
        {
            var streets = await _streetRepository.GetByCityIdAsync(cityId);
            StreetsList.Clear();
            foreach (var street in streets)
                StreetsList.Add(street);

            SelectedStreet = null;
        }

        private async Task LoadObjectAsync(ConsumptionObjectDto obj)
        {
            var street = await _streetRepository.GetByIdAsync(obj.StreetId);
            if (street != null)
            {
                var city = await _cityRepository.GetByIdAsync(street.CityId);
                if (city != null)
                {
                    SelectedRegion = Regions.FirstOrDefault(r => r.Id == city.RegionId);

                    if (SelectedRegion != null)
                    {
                        await LoadCitiesByRegionAsync(SelectedRegion.Id);
                        SelectedCity = Cities.FirstOrDefault(c => c.Id == city.Id);

                        if (SelectedCity != null)
                        {
                            await LoadStreetsByCityAsync(SelectedCity.Id);
                            SelectedStreet = StreetsList.FirstOrDefault(s => s.Id == obj.StreetId);
                        }
                    }
                }
            }

            SelectedObjectType = ObjectTypes.FirstOrDefault(t => t.Id == obj.ObjectTypeId);
            HouseNumber = obj.HouseNumber;
            ApartmentNumber = obj.ApartmentNumber;
            TotalArea = obj.TotalArea ?? 0;
            ResidentCount = obj.ResidentCount;
        }

        private async Task AddRegionAsync()
        {
            var editViewModel = new RegionEditViewModel();
            var editView = new Views.Directories.RegionEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnSaved += async (s, e) =>
            {
                await LoadRegionsAsync();

                var addedRegion = Regions.FirstOrDefault(r => r.Name == editViewModel.Name);
                if (addedRegion != null)
                {
                    SelectedRegion = addedRegion;
                }
                editView.Close();
            };

            editView.ShowDialog();
        }

        private async Task AddCityAsync()
        {
            if (SelectedRegion == null)
            {
                MessageBox.Show("Сначала выберите регион", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editViewModel = new CityEditViewModel(SelectedRegion.Id);
            var editView = new Views.Directories.CityEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnSaved += async (s, e) =>
            {
                await LoadCitiesByRegionAsync(SelectedRegion.Id);

                var addedCity = Cities.FirstOrDefault(c => c.Name == editViewModel.Name);
                if (addedCity != null)
                {
                    SelectedCity = addedCity;
                }
                editView.Close();
            };

            editView.ShowDialog();
        }

        private async Task AddStreetAsync()
        {
            if (SelectedCity == null)
            {
                MessageBox.Show("Сначала выберите город", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editViewModel = new StreetEditViewModel(SelectedCity.Id, SelectedCity.Name);
            var editView = new Views.Directories.StreetEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnStreetSaved += async (s, e) =>
            {
                await LoadStreetsByCityAsync(SelectedCity.Id);

                var addedStreet = StreetsList.FirstOrDefault(s => s.Name == editViewModel.Name);
                if (addedStreet != null)
                {
                    SelectedStreet = addedStreet;
                }
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
                {
                    ResidentCountError = "Укажите количество проживающих";
                }
                return;
            }

            if (ResidentCount.Value <= 0)
            {
                ResidentCountError = "Количество проживающих должно быть больше 0";
                return;
            }

            decimal? totalArea = TotalArea;
            if (totalArea.HasValue && totalArea > 0)
            {
                int maxResidents = CalculateMaxResidents(totalArea.Value);

                if (maxResidents < 1)
                {
                    if (SelectedObjectType?.Name == "Частный дом")
                    {
                        ResidentCountError = $"Для частного дома минимальная площадь должна быть не менее 18 м². " +
                                             $"Ваша площадь: {totalArea.Value} м². Увеличьте площадь или измените тип объекта.";
                    }
                    else
                    {
                        ResidentCountError = $"Для данного типа объекта минимальная площадь должна быть не менее 12 м². " +
                                             $"Ваша площадь: {totalArea.Value} м².";
                    }
                    return;
                }

                if (ResidentCount.Value > maxResidents)
                {
                    if (SelectedObjectType?.Name == "Частный дом")
                    {
                        ResidentCountError = $"Согласно санитарным нормам, для частного дома требуется не менее 18 м² на человека.\n" +
                                             $"Ваша площадь: {totalArea.Value} м². Максимум жильцов: {maxResidents}.\n" +
                                             $"Укажите меньше жильцов или увеличьте площадь.";
                    }
                    else
                    {
                        ResidentCountError = $"Согласно санитарным нормам, на одного человека требуется не менее 12 м².\n" +
                                             $"Ваша площадь: {totalArea.Value} м². Максимум жильцов: {maxResidents}.\n" +
                                             $"Укажите меньше жильцов или увеличьте площадь.";
                    }
                }
            }
            else if (ResidentCount.Value > 10)
            {
                ResidentCountError = $"Указано {ResidentCount.Value} человек. " +
                                     $"Пожалуйста, укажите общую площадь помещения для проверки санитарных норм.";
            }
        }

        private int CalculateMaxResidents(decimal totalArea)
        {
            if (SelectedObjectType?.Name == "Частный дом")
            {
                return Math.Max(1, (int)Math.Floor(totalArea / 18m));
            }
            else
            {
                return (int)Math.Floor(totalArea / 12m);
            }
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

        private async Task SaveAsync()
        {
            ValidateResidentCount();

            if (!string.IsNullOrEmpty(ResidentCountError))
            {
                MessageBox.Show(ResidentCountError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ConsumptionObjectDto dto = new()
            {
                Id = _object?.Id ?? 0,
                StreetId = SelectedStreet.Id,
                HouseNumber = HouseNumber,
                ApartmentNumber = ApartmentNumber,
                ObjectTypeId = SelectedObjectType.Id,
                TotalArea = TotalArea,
                ResidentCount = ResidentCount
            };

            if (IsEditMode)
                _objectRepository.Update(dto);
            else
                _objectRepository.Add(dto);

            // ✅ ДОБАВИТЬ ЭТУ СТРОКУ
            OnSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}