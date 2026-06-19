using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Threading.Tasks;
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
        public event EventHandler OnCancelled;

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

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public RegionEditViewModel()
        {
            _regionRepository = new RegionRepository();
            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => !IsLoading);
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke(this, EventArgs.Empty));
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Введите название региона", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await ExecuteAsync(async () =>
            {
                ErrorMessage = string.Empty;

                var dto = new RegionDto
                {
                    Name = Name.Trim(),
                    Code = Code?.Trim()
                };

                await _regionRepository.AddAsync(dto);
                OnRegionSaved?.Invoke(this, EventArgs.Empty);
            }, "Ошибка при сохранении региона");
        }
    }
}
