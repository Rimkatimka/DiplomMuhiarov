using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;

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

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public StreetEditViewModel(int cityId, string cityName = "")
        {
            _streetRepository = new StreetRepository();
            _cityId = cityId;
            _cityName = cityName;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void Save()
        {
            var dto = new StreetDto
            {
                Name = Name,
                CityId = _cityId,
                PostalCode = PostalCode
            };
            _streetRepository.Add(dto);
            OnStreetSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnStreetSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}