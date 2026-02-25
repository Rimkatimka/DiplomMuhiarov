using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private string _searchText;
        private ConsumptionObjectDto _selectedObject;
        private MeterForReadingDto _selectedMeter;
        private decimal _readingValue;
        private DateTime _readingDate;
        private string _message;

        public ObservableCollection<ConsumptionObjectDto> FoundObjects { get; set; }
        public ObservableCollection<MeterForReadingDto> Meters { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                SearchCommand.Execute(null);
            }
        }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                SetProperty(ref _selectedObject, value);
                LoadMeters();
            }
        }

        public MeterForReadingDto SelectedMeter
        {
            get => _selectedMeter;
            set => SetProperty(ref _selectedMeter, value);
        }

        public decimal ReadingValue
        {
            get => _readingValue;
            set => SetProperty(ref _readingValue, value);
        }

        public DateTime ReadingDate
        {
            get => _readingDate;
            set => SetProperty(ref _readingDate, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ClearCommand { get; }

        public MeterReadingInputViewModel()
        {
            _objectRepository = new ConsumptionObjectRepository();
            _readingRepository = new MeterReadingRepository();

            FoundObjects = new ObservableCollection<ConsumptionObjectDto>();
            Meters = new ObservableCollection<MeterForReadingDto>();
            ReadingDate = DateTime.Today;

            SearchCommand = new RelayCommand(_ => SearchObjects());
            SaveCommand = new RelayCommand(_ => SaveReading(), _ => CanSave());
            ClearCommand = new RelayCommand(_ => ClearForm());
        }

        private void SearchObjects()
        {
            FoundObjects.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
                return;

            var all = _objectRepository.GetAll();
            var filtered = all.Where(o =>
                o.Address.Contains(SearchText) ||
                o.Name.Contains(SearchText)).ToList();

            foreach (var obj in filtered)
                FoundObjects.Add(obj);
        }

        private void LoadMeters()
        {
            Meters.Clear();
            if (_selectedObject == null) return;

            var meters = _readingRepository.GetMetersByObjectId(_selectedObject.Id);
            foreach (var m in meters)
                Meters.Add(m);
        }

        private bool CanSave()
        {
            return _selectedMeter != null && _readingValue > 0;
        }

        private void SaveReading()
        {
            var currentUser = AuthService.CurrentUser;
            if (currentUser == null)
            {
                Message = "Ошибка: пользователь не найден";
                return;
            }

            // Простая проверка на аномалию
            if (_selectedMeter.LastReading.HasValue &&
                _readingValue < _selectedMeter.LastReading.Value)
            {
                Message = "Ошибка: новое показание меньше предыдущего";
                return;
            }

            if (_selectedMeter.LastReading.HasValue &&
                _readingValue - _selectedMeter.LastReading.Value > 1000)
            {
                Message = "Предупреждение: аномально высокое потребление!";
                // Можно сохранить, но с предупреждением
            }

            var dto = new MeterReadingInputDto
            {
                MeterId = _selectedMeter.Id,
                ReadingDate = _readingDate,
                Value = _readingValue,
                EnteredByUserId = currentUser.Id,
                ReadingStatusId = 1, // "Введено"
                TariffZone = 1,
                Comment = null
            };

            try
            {
                _readingRepository.Add(dto);
                Message = "Показания успешно сохранены";
                ClearForm();
            }
            catch (Exception ex)
            {
                Message = $"Ошибка: {ex.Message}";
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
            Message = string.Empty;
        }
    }
}
