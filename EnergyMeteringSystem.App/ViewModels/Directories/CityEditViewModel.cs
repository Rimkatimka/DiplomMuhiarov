using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class CityEditViewModel : ViewModelBase
    {
        private readonly CityRepository _cityRepository;
        private readonly RegionRepository _regionRepository;
        private string _name;
        private RegionDto _selectedRegion;
        private readonly int _preselectedRegionId;

        public event EventHandler OnCitySaved;
        public event EventHandler OnCancelled;

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

        public ObservableCollection<RegionDto> Regions { get; } = new();

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public CityEditViewModel(int preselectedRegionId = 0)
        {
            _cityRepository = new CityRepository();
            _regionRepository = new RegionRepository();
            _preselectedRegionId = preselectedRegionId;

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => !IsLoading);
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke(this, EventArgs.Empty));

            _ = LoadRegionsAsync();
        }

        private async Task LoadRegionsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var regions = await _regionRepository.GetAllAsync();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Regions.Clear();
                    foreach (var region in regions)
                        Regions.Add(region);

                    if (_preselectedRegionId > 0)
                        SelectedRegion = Regions.FirstOrDefault(r => r.Id == _preselectedRegionId);
                });
            }, "Ошибка загрузки регионов");
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || SelectedRegion == null)
            {
                MessageBox.Show("Введите название города и выберите регион", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await ExecuteAsync(async () =>
            {
                var dto = new CityDto
                {
                    Name = Name.Trim(),
                    RegionId = SelectedRegion.Id,
                    RegionName = SelectedRegion.Name
                };

                await _cityRepository.AddAsync(dto);
                OnCitySaved?.Invoke(this, EventArgs.Empty);
            }, "Ошибка при сохранении города");
        }
    }
}
