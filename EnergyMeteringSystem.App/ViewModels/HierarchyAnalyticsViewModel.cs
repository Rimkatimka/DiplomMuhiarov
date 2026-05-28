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
        private ObservableCollection<int> _years;
        private ObservableCollection<string> _months;
        private ObservableCollection<RegionAnalyticsDto> _regions;
        private ObservableCollection<RegionDto> _availableRegions;

        private RegionAnalyticsDto _selectedRegion;
        private RegionAnalyticsDto _selectedRegionData;
        private ObservableCollection<ObjectAnalyticsDto> _topObjects;

        private bool _showRegionDetail;

        public ObservableCollection<int> Years { get; set; }
        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<RegionDto> AvailableRegions { get; set; }
        public ObservableCollection<RegionAnalyticsDto> Regions { get; set; }
        public ObservableCollection<ObjectAnalyticsDto> TopObjects { get; set; }

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
                    LoadRegionDetail();
                }
            }
        }

        public RegionAnalyticsDto SelectedRegionData
        {
            get => _selectedRegionData;
            set => SetProperty(ref _selectedRegionData, value);
        }

        public bool ShowRegionDetail
        {
            get => _showRegionDetail;
            set => SetProperty(ref _showRegionDetail, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand BackToRegionsCommand { get; }

        public HierarchyAnalyticsViewModel()
        {
            _repository = new HierarchyAnalyticsRepository();

            Years = new ObservableCollection<int>();
            Months = new ObservableCollection<string>();
            AvailableRegions = new ObservableCollection<RegionDto>();
            Regions = new ObservableCollection<RegionAnalyticsDto>();
            TopObjects = new ObservableCollection<ObjectAnalyticsDto>();

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

                _showRegionDetail = false;
                OnPropertyChanged(nameof(ShowRegionDetail));
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

                var topObjects = _repository.GetTopObjectsByRegion(SelectedRegion.RegionId, _selectedYear, month, 10);
                TopObjects.Clear();
                foreach (var obj in topObjects)
                    TopObjects.Add(obj);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных региона: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToRegions()
        {
            ShowRegionDetail = false;
            SelectedRegionData = null;
            SelectedRegion = null;
        }
    }
}