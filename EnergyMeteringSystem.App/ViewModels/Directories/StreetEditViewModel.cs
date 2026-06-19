using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class StreetEditViewModel : ViewModelBase
    {
        private readonly StreetRepository _streetRepository;
        private readonly CityRepository _cityRepository;
        private readonly int _cityId;
        private string _name;
        private string _postalCode;
        private string _cityName;

        public event EventHandler OnStreetSaved;
        public event EventHandler OnCancelled;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string PostalCode
        {
            get => _postalCode;
            set => SetProperty(ref _postalCode, value);
        }

        public string CityName
        {
            get => _cityName;
            set => SetProperty(ref _cityName, value);
        }

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public StreetEditViewModel(int cityId, string cityName = "")
        {
            _streetRepository = new StreetRepository();
            _cityRepository = new CityRepository();
            _cityId = cityId;
            _cityName = cityName;

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => !IsLoading);
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke(this, EventArgs.Empty));

            if (string.IsNullOrEmpty(_cityName))
                _ = LoadCityNameAsync();
        }

        private async Task LoadCityNameAsync()
        {
            try
            {
                var city = await _cityRepository.GetByIdAsync(_cityId);
                if (city != null)
                    CityName = city.Name;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCityNameAsync: {ex.Message}");
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Введите название улицы", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await ExecuteAsync(async () =>
            {
                var dto = new StreetDto
                {
                    Name = Name.Trim(),
                    CityId = _cityId,
                    PostalCode = PostalCode?.Trim()
                };

                await _streetRepository.AddAsync(dto);
                OnStreetSaved?.Invoke(this, EventArgs.Empty);
            }, "Ошибка при сохранении улицы");
        }
    }
}
