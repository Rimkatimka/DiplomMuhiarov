using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Auth;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingInputViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly MeterReadingRepository _readingRepository;
        private readonly MeterRepository _meterRepository;
        private readonly UserDto _currentUser;

        private string _searchText;
        private ConsumptionObjectDto _selectedObject;
        private MeterForReadingDto _selectedMeter;
        private decimal _readingValue;
        private DateTime _readingDate;
        private string _warningMessage;



        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredObjects { get; set; }
        public ObservableCollection<MeterForReadingDto> Meters { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                System.Diagnostics.Debug.WriteLine($"=== ВЫБРАН ОБЪЕКТ: Id={value?.Id}, Address={value?.Address} ===");

                if (value != null && value.Id > 0)
                {
                    LoadMeters(value.Id);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("!!! ОШИБКА: Объект не выбран или Id=0 !!!");
                }
                OnPropertyChanged(nameof(HasSelectedObject));
            }
        }

        public MeterForReadingDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                SetProperty(ref _selectedMeter, value);
                CheckAnomaly();
                OnPropertyChanged(nameof(HasSelectedMeter));
            }
        }

        public decimal ReadingValue
        {
            get => _readingValue;
            set
            {
                SetProperty(ref _readingValue, value);
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
                SetProperty(ref _warningMessage, value);
                OnPropertyChanged(nameof(HasWarning));
            }
        }

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
            _meterRepository = new MeterRepository();

            Objects = new ObservableCollection<ConsumptionObjectDto>();
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>();
            Meters = new ObservableCollection<MeterForReadingDto>();

            ReadingDate = DateTime.Today;

            SaveCommand = new RelayCommand(_ => SaveReading(), _ => CanSave());
            ClearCommand = new RelayCommand(_ => ClearForm());

            LoadObjects();
        }

        private void LoadObjects()
        {
            var objects = _objectRepository.GetAll();
            Objects.Clear();
            FilteredObjects.Clear();
            foreach (var obj in objects)
            {
                Objects.Add(obj);
                FilteredObjects.Add(obj);
            }
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredObjects.Clear();
                foreach (var obj in Objects)
                    FilteredObjects.Add(obj);
            }
            else
            {
                var lowerSearch = SearchText.ToLower();
                var filtered = Objects.Where(o =>
                    o.Address.ToLower().Contains(lowerSearch)).ToList();
                FilteredObjects.Clear();
                foreach (var obj in filtered)
                    FilteredObjects.Add(obj);
            }
        }

        private void LoadMeters(int objectId)
        {
            Meters.Clear();
            var meters = _meterRepository.GetByObjectId(objectId);
            foreach (var m in meters)
            {
                var lastReading = _readingRepository.GetLastReading(m.Id);

                var meterForReading = new MeterForReadingDto
                {
                    Id = m.Id,
                    SerialNumber = m.SerialNumber,
                    MeterTypeName = m.MeterTypeName,
                    LastReading = lastReading,
                    LastReadingDate = lastReading.HasValue ? (DateTime?)null : null
                };
                Meters.Add(meterForReading);
            }
        }
        private bool CanSave()
        {
            return SelectedMeter != null && ReadingValue > 0;
        }

        private void CheckAnomaly()
        {
            if (SelectedMeter?.LastReading == null) return;

            if (ReadingValue.ToString().Length > 6)
            {
                WarningMessage = "⚠ Значение превышает допустимое количество знаков!";
            }
            else
            {
                decimal difference = ReadingValue - SelectedMeter.LastReading.Value;
                WarningMessage = difference > 1000
                    ? "⚠ Внимание! Аномально высокое потребление!"
                    : difference < 0 ? "⚠ Ошибка! Новое показание меньше предыдущего!" : string.Empty;
            }
        }

        private void SaveReading()
        {
            if (SelectedMeter == null)
            {
                System.Windows.MessageBox.Show("Выберите счетчик", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var dto = new MeterReadingInputDto
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
                System.Windows.MessageBox.Show("Показания успешно сохранены", "Успех",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка",
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
            WarningMessage = string.Empty;
            LoadObjects(); // сброс фильтра
        }
    }
}