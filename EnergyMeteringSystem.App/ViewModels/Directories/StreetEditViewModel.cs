using System;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class StreetEditViewModel : ViewModelBase
    {
        private readonly StreetRepository _streetRepository;
        private string _name;
        private string _postalCode;
        private int _cityId;
        private string _cityName;

        public event EventHandler OnStreetSaved;

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
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
            _cityId = cityId;
            _cityName = cityName;

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private async Task SaveAsync()
        {
            try
            {
                var dto = new StreetDto
                {
                    Name = Name?.Trim(),
                    CityId = _cityId,
                    PostalCode = PostalCode?.Trim()
                };

                await Task.Run(() => _streetRepository.Add(dto));

                OnStreetSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            OnStreetSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}