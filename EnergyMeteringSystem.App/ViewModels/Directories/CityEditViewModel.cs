using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class CityEditViewModel : ViewModelBase
    {
        private readonly CityRepository _cityRepository;
        private readonly RegionRepository _regionRepository;
        private string _name;
        private RegionDto _selectedRegion;
        private ObservableCollection<RegionDto> _regions;
        private int _preselectedRegionId;
        private string _preselectedRegionName;

        public event EventHandler OnCitySaved;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public RegionDto SelectedRegion
        {
            get => _selectedRegion;
            set => SetProperty(ref _selectedRegion, value);
        }

        public ObservableCollection<RegionDto> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Конструктор для добавления города с предвыбранным регионом
        public CityEditViewModel(int preselectedRegionId = 0, string preselectedRegionName = "")
        {
            _cityRepository = new CityRepository();
            _regionRepository = new RegionRepository();
            Regions = new ObservableCollection<RegionDto>();
            _preselectedRegionId = preselectedRegionId;
            _preselectedRegionName = preselectedRegionName;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadRegions();
        }

        private void LoadRegions()
        {
            var regions = _regionRepository.GetAll();
            Regions.Clear();
            foreach (var region in regions)
                Regions.Add(region);

            // Если передан ID региона - выбираем его
            if (_preselectedRegionId > 0)
            {
                SelectedRegion = Regions.FirstOrDefault(r => r.Id == _preselectedRegionId);
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) && SelectedRegion != null;
        }

        private void Save()
        {
            var dto = new CityDto
            {
                Name = Name,
                RegionId = SelectedRegion.Id,
                RegionName = SelectedRegion.Name
            };
            _cityRepository.Add(dto);
            OnCitySaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnCitySaved?.Invoke(this, EventArgs.Empty);
        }
    }
}