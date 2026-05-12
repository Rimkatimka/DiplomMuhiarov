using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private DateTime _contractDate;
        private DateTime _startDate;
        private DateTime? _endDate;
        private string _dateError;

        public event EventHandler OnContractSaved;

        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<TariffDto> Tariffs { get; set; }
        public ObservableCollection<ContractStatusDto> Statuses { get; set; }

        public string ContractNumber { get; set; }

        public DateTime ContractDate
        {
            get => _contractDate;
            set
            {
                SetProperty(ref _contractDate, value);
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                SetProperty(ref _startDate, value);
                ValidateDates();
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

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

        public string DateError
        {
            get => _dateError;
            set => SetProperty(ref _dateError, value);
        }

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

            Objects = new ObservableCollection<ConsumptionObjectDto>();
            Tariffs = new ObservableCollection<TariffDto>();
            Statuses = new ObservableCollection<ContractStatusDto>();

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

        private void ValidateDates()
        {
            if (ContractDate > DateTime.Today)
            {
                DateError = "Дата договора не может быть позже сегодняшнего дня";
                return;
            }

            if (StartDate < ContractDate)
            {
                DateError = "Дата начала не может быть раньше даты договора";
                return;
            }

            if (EndDate.HasValue && EndDate < StartDate)
            {
                DateError = "Дата окончания не может быть раньше даты начала";
                return;
            }

            DateError = string.Empty;
        }

        private void LoadData()
        {
            var objects = _objectRepository.GetAll();
            foreach (var obj in objects)
                Objects.Add(obj);

            var tariffs = _tariffRepository.GetAll();
            foreach (var tariff in tariffs)
                Tariffs.Add(tariff);

            var statuses = _statusRepository.GetAll();
            foreach (var status in statuses)
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
            return Objects.FirstOrDefault(o => o.Id == id);
        }

        private TariffDto FindTariff(int id)
        {
            return Tariffs.FirstOrDefault(t => t.Id == id);
        }

        private ContractStatusDto FindStatus(int id)
        {
            return Statuses.FirstOrDefault(s => s.Id == id);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(ContractNumber) &&
                   SelectedObject != null &&
                   SelectedTariff != null &&
                   SelectedStatus != null &&
                   string.IsNullOrEmpty(DateError);
        }

        private void Save()
        {
            var dto = new ContractDto
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
                _contractRepository.Update(dto);
            else
                _contractRepository.Add(dto);

            OnContractSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnContractSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}