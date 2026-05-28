using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

        public RelayCommand AddCityCommand { get; }
        public RelayCommand AddStreetCommand { get; }

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
                        LoadCitiesByRegion(value.Id);
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
                        LoadStreetsByCity(value.Id);
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
                    SaveCommand?.RaiseCanExecuteChanged();
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
                    SaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set
            {
                SetProperty(ref _houseNumber, value);
                SaveCommand?.RaiseCanExecuteChanged();
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
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand AddRegionCommand { get; }
        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
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

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
            AddRegionCommand = new RelayCommand(_ => AddRegion());
            AddCityCommand = new RelayCommand(_ => AddCity());
            AddStreetCommand = new RelayCommand(_ => AddStreet());

            LoadRegions();
            LoadData();

            if (existingObject != null)
            {
                IsEditMode = true;
                LoadObject(existingObject);
            }
        }

        private void LoadRegions()
        {
            var regions = _regionRepository.GetAll();
            Regions.Clear();
            foreach (var region in regions)
                Regions.Add(region);
        }

        private void LoadCitiesByRegion(int regionId)
        {
            var cities = _cityRepository.GetByRegionId(regionId);
            Cities.Clear();
            foreach (var city in cities)
                Cities.Add(city);

            // Сбрасываем выбранный город, так как список изменился
            SelectedCity = null;
        }

        private void LoadStreetsByCity(int cityId)
        {
            var streets = _streetRepository.GetByCityId(cityId);
            StreetsList.Clear();
            foreach (var street in streets)
                StreetsList.Add(street);

            // Сбрасываем выбранную улицу
            SelectedStreet = null;
        }

        private void AddRegion()
        {
            var editViewModel = new RegionEditViewModel();
            var editView = new Views.Directories.RegionEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnRegionSaved += (s, e) =>
            {
                LoadRegions();

                // Находим добавленный регион и выбираем его
                var addedRegion = Regions.FirstOrDefault(r => r.Name == editViewModel.Name);
                if (addedRegion != null)
                {
                    SelectedRegion = addedRegion;
                }
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void AddCity()
        {
            if (SelectedRegion == null)
            {
                MessageBox.Show("Сначала выберите регион", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Передаём ID выбранного региона в ViewModel
            var editViewModel = new CityEditViewModel(SelectedRegion.Id, SelectedRegion.Name);
            var editView = new Views.Directories.CityEditView();
            editView.DataContext = editViewModel;

            editViewModel.OnCitySaved += (s, e) =>
            {
                // Перезагружаем города для выбранного региона
                LoadCitiesByRegion(SelectedRegion.Id);

                // Находим добавленный город и выбираем его
                var addedCity = Cities.FirstOrDefault(c => c.Name == editViewModel.Name);
                if (addedCity != null)
                {
                    SelectedCity = addedCity;
                }
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void AddStreet()
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

            editViewModel.OnStreetSaved += (s, e) =>
            {
                // Перезагружаем улицы для выбранного города
                LoadStreetsByCity(SelectedCity.Id);

                // Находим добавленную улицу и выбираем её
                var addedStreet = StreetsList.FirstOrDefault(s => s.Name == editViewModel.Name);
                if (addedStreet != null)
                {
                    SelectedStreet = addedStreet;
                }
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void LoadData()
        {
            var types = _typeRepository.GetAll();
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

        private void LoadObject(ConsumptionObjectDto obj)
        {
            _object = obj;

            // Загружаем информацию об объекте
            var street = _streetRepository.GetById(obj.StreetId);
            if (street != null)
            {
                var city = _cityRepository.GetById(street.CityId);
                if (city != null)
                {
                    // Выбираем регион
                    SelectedRegion = Regions.FirstOrDefault(r => r.Id == city.RegionId);

                    // После выбора региона загружаются города, затем выбираем город
                    if (SelectedRegion != null)
                    {
                        LoadCitiesByRegion(SelectedRegion.Id);
                        SelectedCity = Cities.FirstOrDefault(c => c.Id == city.Id);

                        if (SelectedCity != null)
                        {
                            LoadStreetsByCity(SelectedCity.Id);
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
                if (ResidentCount.Value > maxResidents)
                {
                    ResidentCountError = $"Согласно санитарным нормам, при площади {totalArea.Value} м² " +
                                         $"не может проживать более {maxResidents} человек. " +
                                         $"Рекомендуем проверить данные или зарегистрировать перепланировку.";
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
                return (int)Math.Floor(totalArea / 18m);
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

        private void Save()
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
            {
                _objectRepository.Update(dto);
            }
            else
            {
                _objectRepository.Add(dto);
            }

            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}