using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Contracts
{
    public class ContractListViewModel : ViewModelBase
    {
        private readonly ContractRepository _contractRepository;
        private readonly ConsumptionObjectRepository _objectRepository;
        private readonly TariffRepository _tariffRepository;
        private readonly ContractStatusRepository _statusRepository;

        private string _searchText;
        private ContractDto _selectedContract;
        private bool _showOnlyActive;

        public ObservableCollection<ContractDto> Contracts { get; set; }
        public ObservableCollection<ContractDto> FilteredContracts { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public ContractDto SelectedContract
        {
            get => _selectedContract;
            set
            {
                SetProperty(ref _selectedContract, value);
                EditCommand.RaiseCanExecuteChanged();
                TerminateCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ShowOnlyActive
        {
            get => _showOnlyActive;
            set
            {
                SetProperty(ref _showOnlyActive, value);
                ApplyFilter();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand TerminateCommand { get; }

        public ContractListViewModel()
        {
            _contractRepository = new ContractRepository();
            _objectRepository = new ConsumptionObjectRepository();
            _tariffRepository = new TariffRepository();
            _statusRepository = new ContractStatusRepository();

            Contracts = new ObservableCollection<ContractDto>();
            FilteredContracts = new ObservableCollection<ContractDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddContract());
            EditCommand = new RelayCommand(_ => EditContract(), _ => SelectedContract != null);
            TerminateCommand = new RelayCommand(_ => TerminateContract(), _ => CanTerminate());

            LoadData();
        }

        private void LoadData()
        {
            Contracts.Clear();
            var list = _contractRepository.GetAll();
            foreach (var contract in list)
                Contracts.Add(contract);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredContracts.Clear();

            var filtered = Contracts.AsEnumerable();

            // Фильтр по тексту
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c =>
                    c.ContractNumber.Contains(SearchText) ||
                    c.Address.Contains(SearchText));
            }

            // Фильтр по активности
            if (ShowOnlyActive)
            {
                filtered = filtered.Where(c => c.IsActive);
            }

            foreach (var contract in filtered)
                FilteredContracts.Add(contract);
        }

        private bool CanTerminate()
        {
            return SelectedContract != null && SelectedContract.IsActive;
        }

        private void AddContract()
        {
            var editViewModel = new ContractEditViewModel(_objectRepository, _tariffRepository, _statusRepository);
            var editView = new Views.Contracts.ContractEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Новый договор",
                Content = editView,
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnContractSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            window.ShowDialog();
        }

        private void EditContract()
        {
            if (SelectedContract == null) return;

            var editViewModel = new ContractEditViewModel(_objectRepository, _tariffRepository, _statusRepository, SelectedContract);
            var editView = new Views.Contracts.ContractEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Редактирование договора",
                Content = editView,
                Width = 600,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnContractSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            window.ShowDialog();
        }

        private void TerminateContract()
        {
            if (SelectedContract == null) return;

            var result = MessageBox.Show(
                $"Расторгнуть договор {SelectedContract.ContractNumber}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _contractRepository.Terminate(SelectedContract.Id, DateTime.Today);
                LoadData();
            }
        }
    }
}