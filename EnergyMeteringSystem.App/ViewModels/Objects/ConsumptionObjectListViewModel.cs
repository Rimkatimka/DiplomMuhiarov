using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectListViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _repository;
        private string _searchText;
        public RelayCommand<ConsumptionObjectDto> ShowMetersCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }

        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredObjects { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _ = SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }
        private ConsumptionObjectDto _selectedObject;
        public ConsumptionObjectDto SelectedObject
        {
            get => _selectedObject;
            set
            {
                _ = SetProperty(ref _selectedObject, value);
                // Обновление состояния команд, если нужно
                EditCommand?.RaiseCanExecuteChanged();
                DeleteCommand?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand<ConsumptionObjectDto> EditCommand { get; }

        public ConsumptionObjectListViewModel()
        {
            _repository = new ConsumptionObjectRepository();
            Objects = [];
            FilteredObjects = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddObject());
            EditCommand = new RelayCommand<ConsumptionObjectDto>(EditObject);

            ShowMetersCommand = new RelayCommand<ConsumptionObjectDto>(ShowMeters);

            LoadData();
        }

        private void LoadData()
        {
            List<ConsumptionObjectDto> list = _repository.GetAll();
            Objects.Clear();
            foreach (ConsumptionObjectDto obj in list)
            {
                Objects.Add(obj);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredObjects.Clear();

            ObservableCollection<ConsumptionObjectDto> filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Objects
                : [.. Objects.Where(o =>
                        o.Name.Contains(SearchText) ||
                        o.Address.Contains(SearchText))];

            foreach (ConsumptionObjectDto obj in filtered)
            {
                FilteredObjects.Add(obj);
            }
        }

        private void AddObject()
        {
            ConsumptionObjectEditViewModel editViewModel = new();
            Views.Objects.ConsumptionObjectEditView editView = new(editViewModel);
            _ = editView.ShowDialog();
            // Обновить список после закрытия окна
            LoadData();
        }

        private void EditObject(ConsumptionObjectDto obj)
        {
            if (SelectedObject == null)
            {
                return;
            }

            ConsumptionObjectEditViewModel editViewModel = new(SelectedObject);
            Views.Objects.ConsumptionObjectEditView editView = new(editViewModel);
            _ = editView.ShowDialog();

            // Обновить список после закрытия окна
            LoadData();
        }
        private void ShowMeters(ConsumptionObjectDto obj)
        {
            if (obj == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"ShowMeters вызван для объекта: {obj.Address} (ID={obj.Id})");

            Views.Objects.ObjectMetersView view = new(obj);
            Window window = new()
            {
                Title = $"Счетчики - {obj.Address}",
                Content = view,
                Width = 850,
                Height = 550,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            _ = window.ShowDialog();
        }

    }
}
