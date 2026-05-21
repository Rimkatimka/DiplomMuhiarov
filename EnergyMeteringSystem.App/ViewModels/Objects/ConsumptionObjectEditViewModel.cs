using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectEditViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly StreetRepository _streetRepository;
        private readonly ObjectTypeRepository _typeRepository;

        private ConsumptionObjectDto _object;
        private StreetDto _selectedStreet;
        private ObjectTypeDto _selectedObjectType;
        private string _houseNumber;
        private string _apartmentNumber;
        private decimal _totalArea;
        private int? _residentCount;  // ← изменили на int?
        private string _residentCountError;

        public event EventHandler OnObjectSaved;

        public ObservableCollection<StreetDto> Streets { get; set; }
        public ObservableCollection<ObjectTypeDto> ObjectTypes { get; set; }

        public bool IsApartmentNumberEnabled => !IsPrivateHouse;

        public StreetDto SelectedStreet
        {
            get => _selectedStreet;
            set => SetProperty(ref _selectedStreet, value);
        }

        public bool IsPrivateHouse
        {
            get => SelectedObjectType?.Name == "Частный дом";
        }

        // ✅ ЕДИНСТВЕННОЕ свойство ResidentCount (int?)
        public int? ResidentCount
        {
            get => _residentCount;
            set
            {
                if (SetProperty(ref _residentCount, value))
                {
                    ValidateResidentCount();
                    SaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ResidentCountError
        {
            get => _residentCountError;
            set => SetProperty(ref _residentCountError, value);
        }

        public bool HasResidentCountError => !string.IsNullOrEmpty(ResidentCountError);

        private void ValidateResidentCount()
        {
            ResidentCountError = string.Empty;

            if (!ResidentCount.HasValue)
            {
                if (SelectedObjectType?.Name != "Магазин")
                {
                    ResidentCountError = "Укажите количество проживающих";
                }
                return;
            }

            if (ResidentCount.Value <= 0)
            {
                ResidentCountError = "Количество проживающих должно быть больше 0";
                return;
            }

            // Ограничение по площади
            decimal? totalArea = TotalArea;
            if (totalArea.HasValue && totalArea > 0)
            {
                int maxResidents = CalculateMaxResidents(totalArea.Value);
                if (ResidentCount.Value > maxResidents)
                {
                    ResidentCountError = $"Согласно санитарным нормам, при площади {totalArea.Value} м² " +
                                         $"не может проживать более {maxResidents} человек. " +
                                         $"Рекомендуем проверить данные или зарегистрировать перепланировку.";
                }
            }
            else if (ResidentCount.Value > 10)
            {
                ResidentCountError = $"Указано {ResidentCount.Value} человек. " +
                                     $"Пожалуйста, укажите общую площадь помещения для проверки санитарных норм.";
            }
        }

        private int CalculateMaxResidents(decimal totalArea)
        {
            if (SelectedObjectType?.Name == "Частный дом")
            {
                return (int)Math.Floor(totalArea / 18m);
            }
            else
            {
                return (int)Math.Floor(totalArea / 12m);
            }
        }

        public ObjectTypeDto SelectedObjectType
        {
            get => _selectedObjectType;
            set
            {
                if (SetProperty(ref _selectedObjectType, value))
                {
                    OnPropertyChanged(nameof(IsPrivateHouse));
                    OnPropertyChanged(nameof(IsApartmentNumberEnabled));

                    if (IsPrivateHouse)
                    {
                        ApartmentNumber = string.Empty;
                        OnPropertyChanged(nameof(ApartmentNumber));
                    }

                    // Пересчитываем валидацию при смене типа объекта
                    ValidateResidentCount();
                    SaveCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set
            {
                SetProperty(ref _houseNumber, value);
                System.Diagnostics.Debug.WriteLine($"HouseNumber изменён на: '{value}'");
            }
        }

        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set => SetProperty(ref _apartmentNumber, value);
        }

        public decimal TotalArea
        {
            get => _totalArea;
            set
            {
                SetProperty(ref _totalArea, value);
                ValidateResidentCount();  // ← пересчёт при изменении площади
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ConsumptionObjectEditViewModel(ConsumptionObjectDto existingObject = null)
        {
            _objectRepository = new ConsumptionObjectRepository();
            _streetRepository = new StreetRepository();
            _typeRepository = new ObjectTypeRepository();

            Streets = [];
            ObjectTypes = [];

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();

            if (existingObject != null)
            {
                IsEditMode = true;
                LoadObject(existingObject);
            }
        }

        private void LoadData()
        {
            var streets = _streetRepository.GetAll();
            System.Diagnostics.Debug.WriteLine($"Загружено улиц: {streets.Count}");
            Streets.Clear();
            foreach (var street in streets) Streets.Add(street);

            var types = _typeRepository.GetAll();
            System.Diagnostics.Debug.WriteLine($"Загружено типов: {types.Count}");
            ObjectTypes.Clear();
            foreach (var type in types)
            {
                ObjectTypes.Add(new ObjectTypeDto
                {
                    Id = type.Id,
                    Name = type.Name,
                    Description = type.Description
                });
            }
        }

        private void LoadObject(ConsumptionObjectDto obj)
        {
            _object = obj;
            System.Diagnostics.Debug.WriteLine($"LoadObject: obj.Id = {obj.Id}");
            SelectedStreet = Streets.FirstOrDefault(s => s.Id == obj.StreetId);
            SelectedObjectType = ObjectTypes.FirstOrDefault(t => t.Id == obj.ObjectTypeId);
            HouseNumber = obj.HouseNumber;
            ApartmentNumber = obj.ApartmentNumber;
            TotalArea = obj.TotalArea ?? 0;
            ResidentCount = obj.ResidentCount;
        }

        private bool CanSave()
        {
            return SelectedStreet != null &&
                   SelectedObjectType != null &&
                   !string.IsNullOrWhiteSpace(HouseNumber) &&
                   string.IsNullOrEmpty(ResidentCountError);
        }

        private void Save()
        {
            // Финальная проверка
            ValidateResidentCount();

            if (!string.IsNullOrEmpty(ResidentCountError))
            {
                System.Windows.MessageBox.Show(ResidentCountError, "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"HouseNumber из формы: {HouseNumber}");
            System.Diagnostics.Debug.WriteLine($"_object?.Id = {_object?.Id}");

            ConsumptionObjectDto dto = new()
            {
                Id = _object?.Id ?? 0,
                StreetId = SelectedStreet.Id,
                HouseNumber = HouseNumber,
                ApartmentNumber = ApartmentNumber,
                ObjectTypeId = SelectedObjectType.Id,
                TotalArea = TotalArea,
                ResidentCount = ResidentCount
            };

            if (IsEditMode)
            {
                System.Diagnostics.Debug.WriteLine("Вызов Update");
                _objectRepository.Update(dto);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Вызов Add");
                _objectRepository.Add(dto);
            }

            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}