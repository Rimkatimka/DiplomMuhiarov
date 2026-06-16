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

        private int _selectedYear;
        private string _selectedMonthName;
        private RegionAnalyticsDto _selectedRegion;
        private RegionAnalyticsDto _selectedRegionData;
        private CityAnalyticsDto _selectedCity;
        private StreetAnalyticsDto _selectedStreet;
        private bool _showRegionDetail;
        private bool _showCityDetail;
        private bool _showStreetDetail;
        private bool _hasSelectedRegion;

        // Поиск
        private string _searchCities;
        private string _searchStreets;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<RegionAnalyticsDto> Regions { get; set; }
        public ObservableCollection<ObjectAnalyticsDto> TopObjects { get; set; }
        public ObservableCollection<CityAnalyticsDto> Cities { get; set; }
        public ObservableCollection<StreetAnalyticsDto> Streets { get; set; }
        public ObservableCollection<ObjectAnalyticsDto> StreetObjects { get; set; }

        // Отфильтрованные коллекции для поиска
        public ObservableCollection<CityAnalyticsDto> FilteredCities { get; set; }
        public ObservableCollection<StreetAnalyticsDto> FilteredStreets { get; set; }

        // Свойства для выбранных данных
        public RegionAnalyticsDto SelectedRegionData
        {
            get => _selectedRegionData;
            set => SetProperty(ref _selectedRegionData, value);
        }

        public bool HasSelectedRegion
        {
            get => _hasSelectedRegion;
            set => SetProperty(ref _hasSelectedRegion, value);
        }

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
                if (SetProperty(ref _selectedRegion, value) && value != null)
                {
                    LoadRegionDetail();
                }
            }
        }

        public CityAnalyticsDto SelectedCity
        {
            get => _selectedCity;
            set
            {
                if (SetProperty(ref _selectedCity, value) && value != null)
                {
                    LoadCityDetail();
                }
            }
        }

        public StreetAnalyticsDto SelectedStreet
        {
            get => _selectedStreet;
            set
            {
                if (SetProperty(ref _selectedStreet, value) && value != null)
                {
                    LoadStreetDetail();
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

        // Свойства поиска
        public string SearchCities
        {
            get => _searchCities;
            set
            {
                if (SetProperty(ref _searchCities, value))
                    FilterCities();
            }
        }

        public string SearchStreets
        {
            get => _searchStreets;
            set
            {
                if (SetProperty(ref _searchStreets, value))
                    FilterStreets();
            }
        }

        // Команды
        public RelayCommand<RegionAnalyticsDto> SelectRegionCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackToRegionsCommand { get; }
        public ICommand BackToCityCommand { get; }
        public ICommand BackToStreetCommand { get; }

        public HierarchyAnalyticsViewModel()
        {
            _repository = new HierarchyAnalyticsRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            Regions = new ObservableCollection<RegionAnalyticsDto>();
            TopObjects = new ObservableCollection<ObjectAnalyticsDto>();
            Cities = new ObservableCollection<CityAnalyticsDto>();
            Streets = new ObservableCollection<StreetAnalyticsDto>();
            StreetObjects = new ObservableCollection<ObjectAnalyticsDto>();
            FilteredCities = new ObservableCollection<CityAnalyticsDto>();
            FilteredStreets = new ObservableCollection<StreetAnalyticsDto>();

            for (int i = 2020; i <= DateTime.Today.Year; i++)
                Years.Add(i);

            string[] monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                                    "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            foreach (var m in monthNames)
                Months.Add(m);

            _selectedYear = DateTime.Today.Year;
            _selectedMonthName = Months[DateTime.Today.Month - 1];

            // Инициализация команд
            SelectRegionCommand = new RelayCommand<RegionAnalyticsDto>(SelectRegion);
            RefreshCommand = new RelayCommand(_ => LoadData());
            BackToRegionsCommand = new RelayCommand(_ => BackToRegions());
            BackToCityCommand = new RelayCommand(_ => BackToCity());
            BackToStreetCommand = new RelayCommand(_ => BackToStreet());

            LoadData();
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

                // Сброс состояния
                ShowRegionDetail = false;
                ShowCityDetail = false;
                ShowStreetDetail = false;
                SelectedRegionData = null;
                HasSelectedRegion = false;
                SelectedRegion = null;
                SelectedCity = null;
                SelectedStreet = null;
                Cities.Clear();
                Streets.Clear();
                TopObjects.Clear();
                StreetObjects.Clear();
                FilteredCities.Clear();
                FilteredStreets.Clear();

                // Сбрасываем выделение
                foreach (var region in Regions)
                {
                    region.IsSelected = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectRegion(RegionAnalyticsDto region)
        {
            if (region == null) return;

            // Снимаем выделение со всех регионов
            foreach (var r in Regions)
            {
                r.IsSelected = false;
            }

            // Выделяем выбранный
            region.IsSelected = true;

            // Устанавливаем данные
            SelectedRegionData = region;
            HasSelectedRegion = true;

            // Загружаем детали
            SelectedRegion = region;
        }

        private void LoadRegionDetail()
        {
            if (SelectedRegion == null) return;

            try
            {
                int month = Months.IndexOf(SelectedMonthName) + 1;
                var data = _repository.GetAnalyticsByRegionId(SelectedRegion.RegionId, _selectedYear, month);

                if (data == null) return;

                SelectedRegionData = data;
                ShowRegionDetail = true;
                ShowCityDetail = false;
                ShowStreetDetail = false;

                Cities.Clear();
                FilteredCities.Clear();

                if (data.Cities != null)
                {
                    foreach (var city in data.Cities)
                        Cities.Add(city);
                }

                FilterCities();

                var topObjects = _repository.GetTopObjectsByRegion(SelectedRegion.RegionId, _selectedYear, month, 10);
                TopObjects.Clear();
                foreach (var obj in topObjects)
                {
                    obj.Percentage = SelectedRegionData.TotalConsumption > 0
                        ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                        : 0;
                    TopObjects.Add(obj);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных региона: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCityDetail()
        {
            if (SelectedCity == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== LoadCityDetail: {SelectedCity.CityName} ===");
                System.Diagnostics.Debug.WriteLine($"Streets count in city: {SelectedCity.Streets?.Count ?? 0}");

                ShowCityDetail = true;
                ShowStreetDetail = false;
                SelectedStreet = null;

                Streets.Clear();
                FilteredStreets.Clear();

                if (SelectedCity.Streets != null && SelectedCity.Streets.Any())
                {
                    foreach (var street in SelectedCity.Streets)
                    {
                        Streets.Add(street);
                        System.Diagnostics.Debug.WriteLine($"  Street added: {street.StreetName}, Objects: {street.Objects?.Count ?? 0}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  НЕТ УЛИЦ В ГОРОДЕ!");
                }

                FilterStreets();

                System.Diagnostics.Debug.WriteLine($"Streets after filter: {FilteredStreets.Count}");
                System.Diagnostics.Debug.WriteLine($"Streets in collection: {Streets.Count}");

                StreetObjects.Clear();
                var allObjects = new ObservableCollection<ObjectAnalyticsDto>();

                if (SelectedCity.Streets != null)
                {
                    foreach (var street in SelectedCity.Streets)
                    {
                        if (street.Objects != null)
                        {
                            foreach (var obj in street.Objects)
                            {
                                obj.Percentage = SelectedRegionData?.TotalConsumption > 0
                                    ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                                    : 0;
                                allObjects.Add(obj);
                            }
                        }
                    }
                }

                foreach (var obj in allObjects.OrderByDescending(o => o.Consumption).Take(10))
                {
                    StreetObjects.Add(obj);
                }

                System.Diagnostics.Debug.WriteLine($"StreetObjects count: {StreetObjects.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCityDetail ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
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
                StreetObjects.Clear();

                if (SelectedStreet.Objects != null)
                {
                    foreach (var obj in SelectedStreet.Objects.OrderByDescending(o => o.Consumption).Take(10))
                    {
                        obj.Percentage = SelectedRegionData?.TotalConsumption > 0
                            ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                            : 0;
                        StreetObjects.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных улицы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterCities()
        {
            FilteredCities.Clear();

            if (string.IsNullOrWhiteSpace(SearchCities))
            {
                foreach (var city in Cities)
                    FilteredCities.Add(city);
                return;
            }

            var lower = SearchCities.ToLower();
            foreach (var city in Cities.Where(c => c.CityName.ToLower().Contains(lower)))
                FilteredCities.Add(city);
        }

        private void FilterStreets()
        {
            FilteredStreets.Clear();

            System.Diagnostics.Debug.WriteLine($"FilterStreets: Streets.Count={Streets.Count}, SearchStreets='{SearchStreets}'");

            if (string.IsNullOrWhiteSpace(SearchStreets))
            {
                foreach (var street in Streets)
                    FilteredStreets.Add(street);
                System.Diagnostics.Debug.WriteLine($"FilterStreets: added {FilteredStreets.Count} streets");
                return;
            }

            var lower = SearchStreets.ToLower();
            foreach (var street in Streets.Where(s => s.StreetName.ToLower().Contains(lower)))
                FilteredStreets.Add(street);

            System.Diagnostics.Debug.WriteLine($"FilterStreets: filtered to {FilteredStreets.Count} streets");
        }

        private void BackToRegions()
        {
            ShowRegionDetail = false;
            ShowCityDetail = false;
            ShowStreetDetail = false;
            SelectedRegionData = null;
            HasSelectedRegion = false;
            SelectedRegion = null;
            SelectedCity = null;
            SelectedStreet = null;
            Cities.Clear();
            FilteredCities.Clear();
            Streets.Clear();
            FilteredStreets.Clear();
            TopObjects.Clear();
            StreetObjects.Clear();

            foreach (var region in Regions)
            {
                region.IsSelected = false;
            }

            SearchCities = string.Empty;
            SearchStreets = string.Empty;
        }

        private void BackToCity()
        {
            ShowCityDetail = false;
            ShowStreetDetail = false;
            SelectedStreet = null;
            Streets.Clear();
            FilteredStreets.Clear();
            StreetObjects.Clear();
            SearchStreets = string.Empty;
        }

        private void BackToStreet()
        {
            ShowStreetDetail = false;
            StreetObjects.Clear();
        }
    }
}