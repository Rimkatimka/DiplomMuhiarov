using System;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Auth;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingInputViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterReadingRepository _readingRepository;
        private readonly AuthService _authService;

        private string _searchText;
        private ConsumptionObjectDto _selectedObject;
        private MeterForReadingDto _selectedMeter;
        private decimal _readingValue;
        private DateTime _readingDate;
        private string _warningMessage;
        private readonly UserDto _currentUser;

        public ObservableCollection<ConsumptionObjectDto> FoundObjects { get; set; }
        public ObservableCollection<MeterForReadingDto> Meters { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _ = SetProperty(ref _searchText, value);
                SearchObjects();
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                _ = SetProperty(ref _selectedObject, value);
                OnPropertyChanged(nameof(HasSelectedObject));

                if (value != null)
                {
                    LoadMeters();
                }
            }
        }

        public MeterForReadingDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                _ = SetProperty(ref _selectedMeter, value);
                OnPropertyChanged(nameof(HasSelectedMeter));
            }
        }

        public decimal ReadingValue
        {
            get => _readingValue;
            set
            {
                _ = SetProperty(ref _readingValue, value);
                CheckAnomaly();
            }
        }

        public DateTime ReadingDate
        {
            get => _readingDate;
            set => SetProperty(ref _readingDate, value);
        }

        public string WarningMessage
        {
            get => _warningMessage;
            set
            {
                _ = SetProperty(ref _warningMessage, value);
                OnPropertyChanged(nameof(HasWarning));
            }
        }

        // Свойства для видимости в XAML
        public bool HasSelectedObject => SelectedObject != null;
        public bool HasSelectedMeter => SelectedMeter != null;
        public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);

        public RelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }

        public MeterReadingInputViewModel(UserDto currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _objectRepository = new ConsumptionObjectRepository();
            _readingRepository = new MeterReadingRepository();

            FoundObjects = [];
            Meters = [];
            ReadingDate = DateTime.Today;

            SaveCommand = new RelayCommand(_ => SaveReading(), _ => CanSave());
            ClearCommand = new RelayCommand(_ => ClearForm());
        }

        private void SearchObjects()
        {
            FoundObjects.Clear();

            if (string.IsNullOrWhiteSpace(SearchText) || SearchText.Length < 2)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Поиск: '{SearchText}'");

            System.Collections.Generic.List<ConsumptionObjectDto> all = _objectRepository.GetAll();
            System.Diagnostics.Debug.WriteLine($"Всего объектов: {all.Count}");

            if (all.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("В БД нет объектов!");
                return;
            }

            System.Collections.Generic.List<ConsumptionObjectDto> filtered = all.Where(o =>
                !string.IsNullOrEmpty(o.Address) &&
                o.Address.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();

            System.Diagnostics.Debug.WriteLine($"Найдено объектов: {filtered.Count}");

            foreach (ConsumptionObjectDto obj in filtered)
            {
                FoundObjects.Add(obj);
            }
        }

        private void LoadMeters()
        {
            Meters.Clear();
            if (_selectedObject == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Загрузка счетчиков для объекта ID={_selectedObject.Id}");

            System.Collections.Generic.List<MeterForReadingDto> meters = _readingRepository.GetMetersByObjectId(_selectedObject.Id);

            System.Diagnostics.Debug.WriteLine($"Найдено счетчиков: {meters.Count}");

            foreach (MeterForReadingDto m in meters)
            {
                Meters.Add(m);
            }
        }

        private bool CanSave()
        {
            return SelectedMeter != null && ReadingValue > 0;
        }

        private void CheckAnomaly()
        {
            if (SelectedMeter?.LastReading == null)
            {
                return;
            }

            if (ReadingValue.ToString().Length > 6)
            {
                WarningMessage = "⚠ Значение превышает допустимое количество знаков!";
            }
            decimal difference = ReadingValue - SelectedMeter.LastReading.Value;

            WarningMessage = difference > 1000
                ? "⚠ Внимание! Аномально высокое потребление!"
                : difference < 0 ? "⚠ Ошибка! Новое показание меньше предыдущего!" : string.Empty;
        }

        private void SaveReading()
        {
            if (_currentUser == null)
            {
                _ = System.Windows.MessageBox.Show("Ошибка: пользователь не авторизован", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if (SelectedMeter == null)
            {
                _ = System.Windows.MessageBox.Show("Выберите счетчик", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            MeterReadingInputDto dto = new()
            {
                MeterId = SelectedMeter.Id,
                ReadingDate = ReadingDate,
                Value = ReadingValue,
                EnteredByUserId = _currentUser.Id,
                ReadingStatusId = 1,
                RejectionReasonId = null,
                TariffZone = 1,
                Comment = null
            };

            try
            {
                _readingRepository.Add(dto);
                _ = System.Windows.MessageBox.Show("Показания успешно сохранены", "Успех",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                ClearForm();
            }
            catch (InvalidOperationException ex)
            {
                _ = System.Windows.MessageBox.Show(ex.Message, "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _ = System.Windows.MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            SearchText = string.Empty;
            SelectedObject = null;
            SelectedMeter = null;
            ReadingValue = 0;
            ReadingDate = DateTime.Today;
            FoundObjects.Clear();
            Meters.Clear();
            WarningMessage = string.Empty;

            OnPropertyChanged(nameof(HasSelectedObject));
            OnPropertyChanged(nameof(HasSelectedMeter));
            OnPropertyChanged(nameof(HasWarning));
        }
    }
}