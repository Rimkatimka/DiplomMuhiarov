using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class CityEditViewModel : EditViewModelBase<CityDto, CityRepository>
    {
        private readonly RegionRepository _regionRepository;
        private ObservableCollection<RegionDto> _regions;
        private RegionDto _selectedRegion;
        private int _preselectedRegionId;

        public ObservableCollection<RegionDto> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public RegionDto SelectedRegion
        {
            get => _selectedRegion;
            set => SetProperty(ref _selectedRegion, value);
        }

        public string Name
        {
            get => _originalItem?.Name ?? string.Empty;
            set
            {
                if (_originalItem != null)
                    _originalItem.Name = value;
                OnPropertyChanged();
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Конструктор для добавления города
        public CityEditViewModel(int preselectedRegionId = 0)
            : base(new CityRepository(), null)
        {
            _regionRepository = new RegionRepository();
            Regions = new ObservableCollection<RegionDto>();
            _preselectedRegionId = preselectedRegionId;
            Title = "Добавление города";

            _ = LoadRegionsAsync();
        }

        // Конструктор для редактирования (если понадобится)
        public CityEditViewModel(CityDto existingCity)
            : base(new CityRepository(), existingCity)
        {
            _regionRepository = new RegionRepository();
            Regions = new ObservableCollection<RegionDto>();
            Title = "Редактирование города";

            _ = LoadRegionsAsync();
        }

        private async Task LoadRegionsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var regions = await _regionRepository.GetAllAsync();
                Regions.Clear();
                foreach (var region in regions)
                    Regions.Add(region);

                if (_preselectedRegionId > 0)
                {
                    SelectedRegion = Regions.FirstOrDefault(r => r.Id == _preselectedRegionId);
                }
                else if (_originalItem != null && _originalItem.RegionId > 0)
                {
                    SelectedRegion = Regions.FirstOrDefault(r => r.Id == _originalItem.RegionId);
                }
            }, "Ошибка загрузки регионов");
        }

        protected override void LoadItem(CityDto item)
        {
            Name = item.Name;
            // Регион загрузится асинхронно после загрузки списка
        }

        protected override CityDto GetDto()
        {
            return new CityDto
            {
                Id = _originalItem?.Id ?? 0,
                Name = Name,
                RegionId = SelectedRegion?.Id ?? 0,
                RegionName = SelectedRegion?.Name
            };
        }

        protected override async Task SaveToRepositoryAsync(CityDto dto)
        {
            if (IsEditMode)
                await _repository.UpdateAsync(dto);
            else
                await _repository.AddAsync(dto);
        }

        protected override bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name) && SelectedRegion != null;
        }
    }
}