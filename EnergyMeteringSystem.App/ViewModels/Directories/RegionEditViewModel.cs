using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class RegionEditViewModel : ViewModelBase
    {
        private readonly RegionRepository _regionRepository;
        private string _name;
        private string _code;
        private string _errorMessage;

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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

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
            try
            {
                ErrorMessage = string.Empty;

                var dto = new RegionDto
                {
                    Name = Name.Trim(),
                    Code = Code?.Trim()
                };

                _regionRepository.Add(dto);
                OnRegionSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(ErrorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка при сохранении: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            OnRegionSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}