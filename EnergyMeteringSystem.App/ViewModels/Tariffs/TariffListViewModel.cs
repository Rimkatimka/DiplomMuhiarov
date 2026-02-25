using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Tariffs
{
    public class TariffListViewModel : ViewModelBase
    {
        private readonly TariffRepository _tariffRepository;
        private readonly TariffTypeRepository _typeRepository;

        private string _searchText;
        private TariffDto _selectedTariff;
        private bool _showOnlyActive;

        public ObservableCollection<TariffDto> Tariffs { get; set; }
        public ObservableCollection<TariffDto> FilteredTariffs { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public TariffDto SelectedTariff
        {
            get => _selectedTariff;
            set
            {
                SetProperty(ref _selectedTariff, value);
                EditCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
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
        public RelayCommand DeactivateCommand { get; }

        public TariffListViewModel()
        {
            _tariffRepository = new TariffRepository();
            _typeRepository = new TariffTypeRepository();

            Tariffs = new ObservableCollection<TariffDto>();
            FilteredTariffs = new ObservableCollection<TariffDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddTariff());
            EditCommand = new RelayCommand(_ => EditTariff(), _ => SelectedTariff != null);
            DeactivateCommand = new RelayCommand(_ => DeactivateTariff(), _ => CanDeactivate());

            LoadData();
        }

        private void LoadData()
        {
            Tariffs.Clear();
            var list = _tariffRepository.GetAll();
            foreach (var tariff in list)
                Tariffs.Add(tariff);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredTariffs.Clear();

            var filtered = Tariffs.AsEnumerable();

            // Фильтр по тексту
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    t.TariffTypeName.Contains(SearchText) ||
                    t.ZoneName.Contains(SearchText));
            }

            // Фильтр по активности
            if (ShowOnlyActive)
            {
                filtered = filtered.Where(t => t.IsActive);
            }

            foreach (var tariff in filtered)
                FilteredTariffs.Add(tariff);
        }

        private bool CanDeactivate()
        {
            return SelectedTariff != null && SelectedTariff.IsActive;
        }

        private void AddTariff()
        {
            var types = _typeRepository.GetAll();
            var editViewModel = new TariffEditViewModel(types);
            var editView = new Views.Tariffs.TariffEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Новый тариф",
                Content = editView,
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnTariffSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            window.ShowDialog();
        }

        private void EditTariff()
        {
            if (SelectedTariff == null) return;

            var types = _typeRepository.GetAll();
            var editViewModel = new TariffEditViewModel(types, SelectedTariff);
            var editView = new Views.Tariffs.TariffEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Редактирование тарифа",
                Content = editView,
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnTariffSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            window.ShowDialog();
        }

        private void DeactivateTariff()
        {
            if (SelectedTariff == null) return;

            var result = MessageBox.Show(
                $"Деактивировать тариф {SelectedTariff.TariffTypeName} ({SelectedTariff.ZoneName})?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _tariffRepository.Deactivate(SelectedTariff.Id, DateTime.Today.AddDays(-1));
                LoadData();
            }
        }
    }
}