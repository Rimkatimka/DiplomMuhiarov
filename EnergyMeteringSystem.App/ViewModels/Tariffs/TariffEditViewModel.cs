using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Tariffs
{
    public class TariffEditViewModel : ViewModelBase
    {
        private readonly TariffRepository _tariffRepository;
        private readonly TariffTypeRepository _tariffTypeRepository;
        private TariffDto _tariff;

        public event EventHandler OnTariffSaved;

        public ObservableCollection<TariffTypeDto> TariffTypes { get; set; }

        private TariffTypeDto _selectedTariffType;
        public TariffTypeDto SelectedTariffType
        {
            get => _selectedTariffType;
            set
            {
                SetProperty(ref _selectedTariffType, value);
                OnPropertyChanged(nameof(IsZoneVisible));
                OnPropertyChanged(nameof(ZoneLabel));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private int _zoneNumber;
        public int ZoneNumber
        {
            get => _zoneNumber;
            set
            {
                SetProperty(ref _zoneNumber, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public string ZoneName => ZoneNumber == 1 ? "День" : "Ночь";

        private decimal _pricePerUnit;
        public decimal PricePerUnit
        {
            get => _pricePerUnit;
            set
            {
                SetProperty(ref _pricePerUnit, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

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

        // Свойства для управления видимостью зоны (для двухтарифных тарифов)
        public bool IsZoneVisible => SelectedTariffType?.ZoneCount > 1;
        public string ZoneLabel => SelectedTariffType?.ZoneCount == 2 ? "Зона:" : "Тип тарифа:";

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

        public TariffEditViewModel(TariffDto existingTariff = null)
        {
            _tariffRepository = new TariffRepository();
            _tariffTypeRepository = new TariffTypeRepository();
            TariffTypes = new ObservableCollection<TariffTypeDto>();

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadTariffTypes();

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
                PricePerUnit = 0;
            }
        }

        private void LoadTariffTypes()
        {
            try
            {
                // Получаем List<DirectoryDto> из репозитория
                var types = _tariffTypeRepository.GetAll();
                TariffTypes.Clear();

                foreach (var type in types)
                {
                    // DirectoryDto не имеет ZoneCount, нужно получить его из другого источника
                    // Пока ставим значение по умолчанию
                    int zoneCount = 1; // значение по умолчанию

                    // Если нужно реальное значение ZoneCount, нужно создать отдельный репозиторий
                    // или получить из базы напрямую

                    TariffTypes.Add(new TariffTypeDto
                    {
                        Id = type.Id,
                        Name = type.Name,
                        ZoneCount = zoneCount,  // временно 1
                        Description = type.Description
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки типов тарифов: {ex.Message}");
                MessageBox.Show("Ошибка загрузки типов тарифов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
            return TariffTypes.FirstOrDefault(t => t.Id == id);
        }

        private bool CanSave()
        {
            // Для двухтарифного тарифа зона обязательна (ZoneNumber должен быть 1 или 2)
            if (SelectedTariffType != null && SelectedTariffType.ZoneCount > 1 && (ZoneNumber != 1 && ZoneNumber != 2))
                return false;

            return SelectedTariffType != null &&
                   PricePerUnit > 0 &&
                   StartDate.HasValue &&
                   string.IsNullOrEmpty(DateError);
        }

        private void Save()
        {
            // Финальная валидация перед сохранением
            if (!CanSave())
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TariffDto dto = new()
            {
                Id = _tariff?.Id ?? 0,
                TariffTypeId = SelectedTariffType.Id,
                ZoneNumber = ZoneNumber,
                PricePerUnit = PricePerUnit,
                StartDate = StartDate.Value,
                EndDate = EndDate
            };

            try
            {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения тарифа: {ex.Message}");
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            OnTariffSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}