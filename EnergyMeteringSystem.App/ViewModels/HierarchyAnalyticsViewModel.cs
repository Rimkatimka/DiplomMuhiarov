using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly RegionRepository _regionRepository;

        private int _selectedYear;
        private string _selectedMonthName;
        private RegionAnalyticsDto _selectedRegion;
        private RegionAnalyticsDto _selectedRegionData;
        private CityAnalyticsDto _selectedCity;
        private StreetAnalyticsDto _selectedStreet;
        private bool _showRegionDetail;
        private bool _showCityDetail;
        private bool _showStreetDetail;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
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
                    _ = LoadDataAsync();
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
                        _ = LoadDataAsync();
                }
            }
        }

        public RegionAnalyticsDto SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value) && value != null)
                    _ = LoadRegionDetailAsync();
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
                if (SetProperty(ref _selectedCity, value) && value != null)
                    LoadCityDetail();
            }
        }

        public StreetAnalyticsDto SelectedStreet
        {
            get => _selectedStreet;
            set
            {
                if (SetProperty(ref _selectedStreet, value) && value != null)
                    LoadStreetDetail();
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

        public AsyncRelayCommand RefreshCommand { get; }
        public RelayCommand BackToRegionsCommand { get; }
        public RelayCommand BackToCityCommand { get; }
        public RelayCommand BackToStreetCommand { get; }

        public HierarchyAnalyticsViewModel()
        {
            _repository = new HierarchyAnalyticsRepository();
            _regionRepository = new RegionRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
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

            RefreshCommand = new AsyncRelayCommand(async () => await LoadDataAsync());
            BackToRegionsCommand = new RelayCommand(_ => BackToRegions());
            BackToCityCommand = new RelayCommand(_ => BackToCity());
            BackToStreetCommand = new RelayCommand(_ => BackToStreet());

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                int month = Months.IndexOf(SelectedMonthName) + 1;
                var data = await _repository.GetAnalyticsByRegionAsync(_selectedYear, month);

                Regions.Clear();
                foreach (var item in data)
                    Regions.Add(item);

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
            }, "Ошибка загрузки данных");
        }

        private async Task LoadRegionDetailAsync()
        {
            if (SelectedRegion == null) return;

            await ExecuteAsync(async () =>
            {
                int month = Months.IndexOf(SelectedMonthName) + 1;
                var data = await _repository.GetAnalyticsByRegionIdAsync(SelectedRegion.RegionId, _selectedYear, month);

                SelectedRegionData = data;
                ShowRegionDetail = true;

                // ТОП-10 объектов региона
                var topObjects = await _repository.GetTopObjectsByRegionAsync(SelectedRegion.RegionId, _selectedYear, month, 10);
                TopObjects.Clear();
                foreach (var obj in topObjects)
                {
                    string cityName = GetCityNameByStreet(obj.Address);
                    obj.Address = $"{cityName}, {obj.Address}";
                    obj.Percentage = SelectedRegionData.TotalConsumption > 0
                        ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                        : 0;
                    TopObjects.Add(obj);
                }

                // Обогащаем адреса в иерархии
                if (SelectedRegionData?.Cities != null)
                {
                    foreach (var city in SelectedRegionData.Cities)
                    {
                        foreach (var street in city.Streets)
                        {
                            foreach (var obj in street.Objects)
                            {
                                obj.Address = $"{city.CityName}, {street.StreetName}";
                                obj.Percentage = SelectedRegionData.TotalConsumption > 0
                                    ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                                    : 0;
                            }
                        }
                    }
                }
            }, "Ошибка загрузки данных региона");
        }

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

                SelectedStreets.Clear();
                if (SelectedCity.Streets != null)
                {
                    foreach (var street in SelectedCity.Streets)
                        SelectedStreets.Add(street);
                }

                CityTopObjects.Clear();

                var allObjects = new ObservableCollection<ObjectAnalyticsDto>();
                foreach (var street in SelectedCity.Streets)
                {
                    foreach (var obj in street.Objects)
                    {
                        obj.Percentage = SelectedRegionData?.TotalConsumption > 0
                            ? (obj.Consumption / SelectedRegionData.TotalConsumption) * 100
                            : 0;
                        allObjects.Add(obj);
                    }
                }

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
                StreetTopObjects.Clear();

                foreach (var obj in SelectedStreet.Objects.OrderByDescending(o => o.Consumption).Take(10))
                {
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