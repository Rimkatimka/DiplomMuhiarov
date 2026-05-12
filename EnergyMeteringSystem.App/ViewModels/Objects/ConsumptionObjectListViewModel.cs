using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.Views.Objects;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

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
                SetProperty(ref _selectedObject, value);
                System.Diagnostics.Debug.WriteLine($"SelectedObject установлен: Id={value?.Id}");
                // Не вызывай здесь LoadData!
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
            Objects.Clear();
            var list = _repository.GetAll();
            
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>(Objects);
            OnPropertyChanged(nameof(FilteredObjects));
            foreach (var obj in list)
                Objects.Add(obj);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredObjects.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Objects
                : new ObservableCollection<ConsumptionObjectDto>(
                    Objects.Where(o => o.Address.Contains(SearchText)));

            foreach (var obj in filtered)
                FilteredObjects.Add(obj);

            System.Diagnostics.Debug.WriteLine($"FilteredObjects: {FilteredObjects.Count} объектов");
            OnPropertyChanged(nameof(FilteredObjects));  // ← добавить!
        }

        private void AddObject()
        {
            var editViewModel = new ConsumptionObjectEditViewModel();
            var editView = new ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);


            editViewModel.OnObjectSaved += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("OnObjectSaved сработал");
                LoadData();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void EditObject(ConsumptionObjectDto obj)
        {
            if (obj == null) return;

            var editViewModel = new ConsumptionObjectEditViewModel(obj);
            var editView = new ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            editViewModel.OnObjectSaved += (s, e) =>
            {
                LoadData();
                editView.Close();
            };

            editView.ShowDialog();
        }
        private void ShowMeters(ConsumptionObjectDto obj)
        {
            if (obj == null) return;

            var window = new Views.Meters.MetersForObjectView();
            var viewModel = new ViewModels.Meters.MetersForObjectViewModel(obj);
            window.DataContext = viewModel;
            window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            window.ShowDialog();
        }

    }
}
