using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class RegionEditViewModel : ViewModelBase
    {
        private readonly RegionRepository _regionRepository;
        private string _name;
        private string _code;

        public event EventHandler OnRegionSaved;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RegionEditViewModel()
        {
            _regionRepository = new RegionRepository();
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        private void Save()
        {
            var dto = new RegionDto
            {
                Name = Name,
                Code = Code
            };
            _regionRepository.Add(dto);
            OnRegionSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnRegionSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}