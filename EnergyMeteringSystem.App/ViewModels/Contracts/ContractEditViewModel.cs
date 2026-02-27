using System;
using System.Collections.ObjectModel;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Contracts
{
    public class ContractEditViewModel : ViewModelBase
    {
        private readonly ContractRepository _contractRepository;
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly TariffRepository _tariffRepository;
        private readonly ContractStatusRepository _statusRepository;

        private ContractDto _contract;
        private ConsumptionObjectDto _selectedObject;
        private TariffDto _selectedTariff;
        private ContractStatusDto _selectedStatus;

        public event EventHandler OnContractSaved;

        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<TariffDto> Tariffs { get; set; }
        public ObservableCollection<ContractStatusDto> Statuses { get; set; }

        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set => SetProperty(ref _selectedObject, value);
        }

        public TariffDto SelectedTariff
        {
            get => _selectedTariff;
            set => SetProperty(ref _selectedTariff, value);
        }

        public ContractStatusDto SelectedStatus
        {
            get => _selectedStatus;
            set => SetProperty(ref _selectedStatus, value);
        }

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ContractEditViewModel(
            ConsumptionObjectRepository objectRepo,
            TariffRepository tariffRepo,
            ContractStatusRepository statusRepo,
            ContractDto existingContract = null)
        {
            _contractRepository = new ContractRepository();
            _objectRepository = objectRepo;
            _tariffRepository = tariffRepo;
            _statusRepository = statusRepo;

            Objects = [];
            Tariffs = [];
            Statuses = [];

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();

            if (existingContract != null)
            {
                IsEditMode = true;
                LoadContract(existingContract);
            }
            else
            {
                IsEditMode = false;
                ContractDate = DateTime.Today;
                StartDate = DateTime.Today;
            }
        }

        private void LoadData()
        {
            // Загрузка объектов
            System.Collections.Generic.List<ConsumptionObjectDto> objects = _objectRepository.GetAll();
            foreach (ConsumptionObjectDto obj in objects)
            {
                Objects.Add(obj);
            }

            // Загрузка тарифов
            System.Collections.Generic.List<TariffDto> tariffs = _tariffRepository.GetAll();
            foreach (TariffDto tariff in tariffs)
            {
                Tariffs.Add(tariff);
            }

            // Загрузка статусов — преобразуем DirectoryDto в ContractStatusDto
            System.Collections.Generic.List<DirectoryDto> statuses = _statusRepository.GetAll();
            foreach (DirectoryDto status in statuses)
            {
                Statuses.Add(new ContractStatusDto
                {
                    Id = status.Id,
                    Name = status.Name,
                    Description = status.Description
                });
            }
        }

        private void LoadContract(ContractDto contract)
        {
            _contract = contract;

            ContractNumber = contract.ContractNumber;
            ContractDate = contract.ContractDate;
            StartDate = contract.StartDate;
            EndDate = contract.EndDate;

            SelectedObject = FindObject(contract.ConsumptionObjectId);
            SelectedTariff = FindTariff(contract.TariffId);
            SelectedStatus = FindStatus(contract.ContractStatusId);
        }

        private ConsumptionObjectDto FindObject(int id)
        {
            foreach (ConsumptionObjectDto obj in Objects)
            {
                if (obj.Id == id)
                {
                    return obj;
                }
            }

            return null;
        }

        private TariffDto FindTariff(int id)
        {
            foreach (TariffDto tariff in Tariffs)
            {
                if (tariff.Id == id)
                {
                    return tariff;
                }
            }

            return null;
        }

        private ContractStatusDto FindStatus(int id)
        {
            foreach (ContractStatusDto status in Statuses)
            {
                if (status.Id == id)
                {
                    return status;
                }
            }

            return null;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ContractNumber) &&
                   SelectedObject != null &&
                   SelectedTariff != null &&
                   SelectedStatus != null;
        }

        private void Save()
        {
            ContractDto dto = new()
            {
                Id = _contract?.Id ?? 0,
                ContractNumber = ContractNumber,
                ConsumptionObjectId = SelectedObject.Id,
                ContractDate = ContractDate,
                StartDate = StartDate,
                EndDate = EndDate,
                ContractStatusId = SelectedStatus.Id,
                TariffId = SelectedTariff.Id
            };

            if (IsEditMode)
            {
                _contractRepository.Update(dto);
            }
            else
            {
                _contractRepository.Add(dto);
            }

            OnContractSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnContractSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}