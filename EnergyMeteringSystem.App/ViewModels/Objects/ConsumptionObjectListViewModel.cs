using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.Views.Objects;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using RelayCommand = EnergyMeteringSystem.App.Commands.RelayCommand;

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
                DeleteCommand?.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand<ConsumptionObjectDto> EditCommand { get; }

        public ConsumptionObjectListViewModel()
        {
            _repository = new ConsumptionObjectRepository();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddObject());
            EditCommand = new RelayCommand<ConsumptionObjectDto>(EditObject);
            ShowMetersCommand = new RelayCommand<ConsumptionObjectDto>(ShowMeters);
            DeleteCommand = new RelayCommand(_ => DeleteObject(), _ => SelectedObject != null);

            LoadData();
        }

        private void LoadData()
        {
            Objects.Clear();
            var list = _repository.GetAll();
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

            OnPropertyChanged(nameof(FilteredObjects));
        }

        private void AddObject()
        {
            var editViewModel = new ConsumptionObjectEditViewModel();
            var editView = new ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            editViewModel.OnObjectSaved += (s, e) =>
            {
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

        private void DeleteObject()
        {
            if (SelectedObject == null) return;

            var result = MessageBox.Show(
                $"Удалить объект \"{SelectedObject.Address}\"?\n\n" +
                "Все связанные данные (счетчики, показания, начисления, платежи) также будут удалены!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.Delete(SelectedObject.Id);
                    LoadData();
                    MessageBox.Show("Объект успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}