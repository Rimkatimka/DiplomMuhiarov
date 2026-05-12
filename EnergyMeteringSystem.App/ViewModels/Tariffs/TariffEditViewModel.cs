using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Tariffs
{
    public class TariffEditViewModel : ViewModelBase
    {
        private readonly TariffRepository _tariffRepository;
        private TariffDto _tariff;

        public event EventHandler OnTariffSaved;

        public ObservableCollection<TariffTypeDto> TariffTypes { get; set; }

        public TariffTypeDto SelectedTariffType { get; set; }
        public int ZoneNumber { get; set; }
        public string ZoneName => ZoneNumber == 1 ? "День" : "Ночь";
        public decimal PricePerUnit { get; set; }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                SetProperty(ref _endDate, value);
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _dateError;
        public string DateError
        {
            get => _dateError;
            set
            {
                if (SetProperty(ref _dateError, value))
                {
                    OnPropertyChanged(nameof(DateErrorVisibility));
                }
            }
        }

        public Visibility DateErrorVisibility => string.IsNullOrEmpty(DateError) ? Visibility.Collapsed : Visibility.Visible;

        private void ValidateDates()
        {
            if (!StartDate.HasValue)
            {
                DateError = "Дата начала обязательна";
                return;
            }

            if (EndDate.HasValue && EndDate < StartDate)
            {
                DateError = "Дата окончания не может быть раньше даты начала";
                return;
            }

            DateError = string.Empty;
        }

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public TariffEditViewModel(ObservableCollection<TariffTypeDto> types, TariffDto existingTariff = null)
        {
            _tariffRepository = new TariffRepository();
            TariffTypes = types;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            if (existingTariff != null)
            {
                IsEditMode = true;
                LoadTariff(existingTariff);
            }
            else
            {
                IsEditMode = false;
                StartDate = DateTime.Today;
                ZoneNumber = 1;
            }
        }

        private void LoadTariff(TariffDto tariff)
        {
            _tariff = tariff;

            SelectedTariffType = FindTariffType(tariff.TariffTypeId);
            ZoneNumber = tariff.ZoneNumber;
            PricePerUnit = tariff.PricePerUnit;
            StartDate = tariff.StartDate;
            EndDate = tariff.EndDate;
        }

        private TariffTypeDto FindTariffType(int id)
        {
            foreach (TariffTypeDto type in TariffTypes)
            {
                if (type.Id == id)
                {
                    return type;
                }
            }

            return null;
        }

        private bool CanSave()
        {
            return SelectedTariffType != null &&
                   PricePerUnit > 0;
        }

        private void Save()
        {
            TariffDto dto = new()
            {
                Id = _tariff?.Id ?? 0,
                TariffTypeId = SelectedTariffType.Id,
                ZoneNumber = ZoneNumber,
                PricePerUnit = PricePerUnit,
                StartDate = (DateTime)StartDate,
                EndDate = EndDate
            };

            if (IsEditMode)
            {
                _tariffRepository.Update(dto);
            }
            else
            {
                _tariffRepository.Add(dto);
            }

            OnTariffSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnTariffSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}