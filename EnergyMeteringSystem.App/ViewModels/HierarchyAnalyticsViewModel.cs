using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Analytics
{
    public class HierarchyAnalyticsViewModel : ViewModelBase
    {
        private readonly HierarchyAnalyticsRepository _repository;
        private readonly CityRepository _cityRepository;
        private readonly StreetRepository _streetRepository;
        private readonly ConsumptionObjectRepository _objectRepository;

        private int _selectedYear;
        private string _selectedMonthName;
        private ObservableCollection<int> _years;
        private ObservableCollection<string> _months;
        private ObservableCollection<RegionAnalyticsDto> _regions;
        private ObservableCollection<RegionDto> _availableRegions;

        private RegionAnalyticsDto _selectedRegion;
        private RegionAnalyticsDto _selectedRegionData;
        private ObservableCollection<ObjectAnalyticsDto> _topObjects;
        private ObservableCollection<CityAnalyticsDto> _selectedCities;
        private ObservableCollection<StreetAnalyticsDto> _selectedStreets;

        private CityAnalyticsDto _selectedCity;
        private StreetAnalyticsDto _selectedStreet;
        private ObservableCollection<ObjectAnalyticsDto> _cityTopObjects;
        private ObservableCollection<ObjectAnalyticsDto> _streetTopObjects;

        private bool _showRegionDetail;
        private bool _showCityDetail;
        private bool _showStreetDetail;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<RegionDto> AvailableRegions { get; set; }
        public ObservableCollection<RegionAnalyticsDto> Regions { get; set; }
        public ObservableCollection<ObjectAnalyticsDto> TopObjects { get; set; }
        public ObservableCollection<CityAnalyticsDto> SelectedCities { get; set; }
        public ObservableCollection<StreetAnalyticsDto> SelectedStreets { get; set; }
        public ObservableCollection<ObjectAnalyticsDto> CityTopObjects { get; set; }
        public ObservableCollection<ObjectAnalyticsDto> StreetTopObjects { get; set; }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                    LoadData();
            }
        }

        public string SelectedMonthName
        {
            get => _selectedMonthName;
            set
            {
                if (SetProperty(ref _selectedMonthName, value))
                {
                    int monthIndex = Months.IndexOf(value) + 1;
                    if (monthIndex > 0)
                        LoadData();
                }
            }
        }

        public RegionAnalyticsDto SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value))
                {
                    if (value != null)
                    {
                        LoadRegionDetail();
                    }
                }
            }
        }

        public RegionAnalyticsDto SelectedRegionData
        {
            get => _selectedRegionData;
            set => SetProperty(ref _selectedRegionData, value);
        }

        public CityAnalyticsDto SelectedCity
        {
            get => _selectedCity;
            set
            {
                if (SetProperty(ref _selectedCity, value))
                {
                    if (value != null)
                    {
                        LoadCityDetail();
                    }
                }
            }
        }

        public StreetAnalyticsDto SelectedStreet
        {
            get => _selectedStreet;
            set
            {
                if (SetProperty(ref _selectedStreet, value))
                {
                    if (value != null)
                    {
                        LoadStreetDetail();
                    }
                }
            }
        }

        public bool ShowRegionDetail
        {
            get => _showRegionDetail;
            set => SetProperty(ref _showRegionDetail, value);
        }

        public bool ShowCityDetail
        {
            get => _showCityDetail;
            set => SetProperty(ref _showCityDetail, value);
        }

        public bool ShowStreetDetail
        {
            get => _showStreetDetail;
            set => SetProperty(ref _showStreetDetail, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackToRegionsCommand { get; }
        public ICommand BackToCityCommand { get; }
        public ICommand BackToStreetCommand { get; }

        public HierarchyAnalyticsViewModel()
        {
            _repository = new HierarchyAnalyticsRepository();
            _cityRepository = new CityRepository();
            _streetRepository = new StreetRepository();
            _objectRepository = new ConsumptionObjectRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            AvailableRegions = new ObservableCollection<RegionDto>();
            Regions = new ObservableCollection<RegionAnalyticsDto>();
            TopObjects = new ObservableCollection<ObjectAnalyticsDto>();
            SelectedCities = new ObservableCollection<CityAnalyticsDto>();
            SelectedStreets = new ObservableCollection<StreetAnalyticsDto>();
            CityTopObjects = new ObservableCollection<ObjectAnalyticsDto>();
            StreetTopObjects = new ObservableCollection<ObjectAnalyticsDto>();

            for (int i = 2020; i <= DateTime.Today.Year; i++)
                Years.Add(i);

            string[] monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                    "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            foreach (var m in monthNames)
                Months.Add(m);

            _selectedYear = DateTime.Today.Year;
            _selectedMonthName = Months[DateTime.Today.Month - 1];

            RefreshCommand = new RelayCommand(_ => LoadData());
            BackToRegionsCommand = new RelayCommand(_ => BackToRegions());
            BackToCityCommand = new RelayCommand(_ => BackToCity());
            BackToStreetCommand = new RelayCommand(_ => BackToStreet());

            LoadRegions();
            LoadData();
        }

        private void LoadRegions()
        {
            var regionRepo = new RegionRepository();
            var regions = regionRepo.GetAll();
            AvailableRegions.Clear();
            foreach (var region in regions)
                AvailableRegions.Add(region);
        }

        private void LoadData()
        {
            try
            {
                int month = Months.IndexOf(SelectedMonthName) + 1;
                var data = _repository.GetAnalyticsByRegion(_selectedYear, month);

                Regions.Clear();
                foreach (var item in data)
                    Regions.Add(item);

                ShowRegionDetail = false;
                ShowCityDetail = false;
                ShowStreetDetail = false;
                SelectedRegionData = null;
                SelectedCities.Clear();
                SelectedStreets.Clear();
                CityTopObjects.Clear();
                StreetTopObjects.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRegionDetail()
        {
            if (SelectedRegion == null) return;

            try
            {
                int month = Months.IndexOf(SelectedMonthName) + 1;
                var data = _repository.GetAnalyticsByRegionId(SelectedRegion.RegionId, _selectedYear, month);

                SelectedRegionData = data;
                ShowRegionDetail = true;

                // ТОП-10 объектов региона с полным адресом
                var topObjects = _repository.GetTopObjectsByRegion(SelectedRegion.RegionId, _selectedYear, month, 10);
                TopObjects.Clear();
                foreach (var obj in topObjects)
                {
                    // Добавляем город к адресу
                    string cityName = GetCityNameByStreet(obj.Address);
                    obj.Address = $"{cityName}, {obj.Address}";
                    obj.Percentage = SelectedRegionData.TotalConsumption > 0
                        ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                        : 0;
                    TopObjects.Add(obj);
                }

                // Добавляем город к адресам в городах и улицах
                if (SelectedRegionData?.Cities != null)
                {
                    foreach (var city in SelectedRegionData.Cities)
                    {
                        foreach (var street in city.Streets)
                        {
                            foreach (var obj in street.Objects)
                            {
                                // Формируем полный адрес: Город, Улица, Дом
                                obj.Address = $"{city.CityName}, {street.StreetName}";
                                obj.Percentage = SelectedRegionData.TotalConsumption > 0
                                    ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                                    : 0;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных региона: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательный метод для получения города по улице
        private string GetCityNameByStreet(string streetName)
        {
            if (SelectedRegionData?.Cities != null)
            {
                foreach (var city in SelectedRegionData.Cities)
                {
                    foreach (var street in city.Streets)
                    {
                        if (street.StreetName == streetName)
                            return city.CityName;
                    }
                }
            }
            return "";
        }

        private void LoadCityDetail()
        {
            if (SelectedCity == null) return;

            try
            {
                ShowCityDetail = true;
                ShowStreetDetail = false;
                SelectedStreet = null;

                // Загружаем улицы города
                SelectedStreets.Clear();
                if (SelectedCity.Streets != null)
                {
                    foreach (var street in SelectedCity.Streets)
                        SelectedStreets.Add(street);
                }

                // ТОП-10 объектов города
                CityTopObjects.Clear();

                // Собираем все объекты города из улиц
                var allObjects = new ObservableCollection<ObjectAnalyticsDto>();
                foreach (var street in SelectedCity.Streets)
                {
                    foreach (var obj in street.Objects)
                    {
                        // Рассчитываем процент от РЕГИОНА (не от города!)
                        obj.Percentage = SelectedRegionData?.TotalConsumption > 0
                            ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                            : 0;
                        allObjects.Add(obj);
                    }
                }

                // Берем топ-10
                foreach (var obj in allObjects.OrderByDescending(o => o.Consumption).Take(10))
                {
                    CityTopObjects.Add(obj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных города: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStreetDetail()
        {
            if (SelectedStreet == null) return;

            try
            {
                ShowStreetDetail = true;

                // ТОП-10 объектов улицы с процентом от РЕГИОНА
                StreetTopObjects.Clear();
                foreach (var obj in SelectedStreet.Objects.OrderByDescending(o => o.Consumption).Take(10))
                {
                    // Рассчитываем процент от региона
                    obj.Percentage = SelectedRegionData?.TotalConsumption > 0
                        ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                        : 0;
                    StreetTopObjects.Add(obj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных улицы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToRegions()
        {
            ShowRegionDetail = false;
            ShowCityDetail = false;
            ShowStreetDetail = false;
            SelectedRegionData = null;
            SelectedRegion = null;
            SelectedCity = null;
            SelectedStreet = null;
            SelectedCities.Clear();
            SelectedStreets.Clear();
            TopObjects.Clear();
            CityTopObjects.Clear();
            StreetTopObjects.Clear();
        }

        private void BackToCity()
        {
            ShowCityDetail = false;
            ShowStreetDetail = false;
            SelectedStreet = null;
            CityTopObjects.Clear();
            StreetTopObjects.Clear();
        }

        private void BackToStreet()
        {
            ShowStreetDetail = false;
            StreetTopObjects.Clear();
        }
    }
}