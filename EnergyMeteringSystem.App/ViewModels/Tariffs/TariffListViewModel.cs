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
                _ = SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public TariffDto SelectedTariff
        {
            get => _selectedTariff;
            set
            {
                _ = SetProperty(ref _selectedTariff, value);
                EditCommand.RaiseCanExecuteChanged();
                DeactivateCommand.RaiseCanExecuteChanged();
            }
        }
        private void AddTariff()
        {
            var editViewModel = new TariffEditViewModel();  // больше не нужно передавать типы
            var editView = new Views.Tariffs.TariffEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Новый тариф",
                Content = editView,
                Width = 500,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnTariffSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            _ = window.ShowDialog();
        }

        private void EditTariff()
        {
            if (SelectedTariff == null) return;

            var editViewModel = new TariffEditViewModel(SelectedTariff);
            var editView = new Views.Tariffs.TariffEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Редактирование тарифа",
                Content = editView,
                Width = 500,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnTariffSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            _ = window.ShowDialog();
        }
        public bool ShowOnlyActive
        {
            get => _showOnlyActive;
            set
            {
                _ = SetProperty(ref _showOnlyActive, value);
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

            Tariffs = [];
            FilteredTariffs = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddTariff());
            EditCommand = new RelayCommand(_ => EditTariff(), _ => SelectedTariff != null);
            DeactivateCommand = new RelayCommand(_ => DeactivateTariff(), _ => CanDeactivate());

            LoadData();
        }

        private void LoadData()
        {
            Tariffs.Clear();
            System.Collections.Generic.List<TariffDto> list = _tariffRepository.GetAll();
            foreach (TariffDto tariff in list)
            {
                Tariffs.Add(tariff);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredTariffs.Clear();

            System.Collections.Generic.IEnumerable<TariffDto> filtered = Tariffs.AsEnumerable();

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

            foreach (TariffDto tariff in filtered)
            {
                FilteredTariffs.Add(tariff);
            }
        }

        private bool CanDeactivate()
        {
            return SelectedTariff != null && SelectedTariff.IsActive;
        }
             
        

        private void DeactivateTariff()
        {
            if (SelectedTariff == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
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