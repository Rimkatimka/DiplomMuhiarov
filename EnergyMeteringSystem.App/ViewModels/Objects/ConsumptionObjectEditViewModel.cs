using System;
using System.Collections.ObjectModel;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

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
        private int _residentCount;

        public event EventHandler OnObjectSaved;

        public ObservableCollection<StreetDto> Streets { get; set; }
        public ObservableCollection<ObjectTypeDto> ObjectTypes { get; set; }

        public StreetDto SelectedStreet
        {
            get => _selectedStreet;
            set => SetProperty(ref _selectedStreet, value);
        }

        public ObjectTypeDto SelectedObjectType
        {
            get => _selectedObjectType;
            set => SetProperty(ref _selectedObjectType, value);
        }

        public string HouseNumber
        {
            get => _houseNumber;
            set => SetProperty(ref _houseNumber, value);
        }

        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set => SetProperty(ref _apartmentNumber, value);
        }

        public decimal TotalArea
        {
            get => _totalArea;
            set => SetProperty(ref _totalArea, value);
        }

        public int ResidentCount
        {
            get => _residentCount;
            set => SetProperty(ref _residentCount, value);
        }

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ConsumptionObjectEditViewModel(ConsumptionObjectDto existingObject = null)
        {
            _objectRepository = new ConsumptionObjectRepository();
            _streetRepository = new StreetRepository();
            _typeRepository = new ObjectTypeRepository();

            Streets = new ObservableCollection<StreetDto>();
            ObjectTypes = new ObservableCollection<ObjectTypeDto>();

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
            // Загрузка улиц
            var streets = _streetRepository.GetAll();
            foreach (var street in streets)
                Streets.Add(street);

            // Загрузка типов объектов - преобразуем DirectoryDto в ObjectTypeDto
            var types = _typeRepository.GetAll(); // это List<DirectoryDto>
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
            SelectedStreet = FindStreet(obj.StreetId);
            HouseNumber = obj.HouseNumber;
            ApartmentNumber = obj.ApartmentNumber;
            SelectedObjectType = FindObjectType(obj.ObjectTypeId);
            TotalArea = obj.TotalArea ?? 0;
            ResidentCount = obj.ResidentCount ?? 0;
        }

        private StreetDto FindStreet(int id)
        {
            foreach (var street in Streets)
                if (street.Id == id) return street;
            return null;
        }

        private ObjectTypeDto FindObjectType(int id)
        {
            foreach (var type in ObjectTypes)
                if (type.Id == id) return type;
            return null;
        }

        private bool CanSave()
        {
            return SelectedStreet != null &&
                   SelectedObjectType != null &&
                   !string.IsNullOrWhiteSpace(HouseNumber);
        }

        private void Save()
        {
            var dto = new ConsumptionObjectDto
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
                _objectRepository.Update(dto);
            else
                _objectRepository.Add(dto);

            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnObjectSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}