using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectListViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _repository;
        private string _searchText;
        private ConsumptionObjectDto _selectedItem;

        public ObservableCollection<ConsumptionObjectDto> Items { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredItems { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public ConsumptionObjectDto SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    // ✅ Обновляем состояние кнопок
                    EditCommand.RaiseCanExecuteChanged();
                    DeleteCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Команды
        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand<ConsumptionObjectDto> ShowMetersCommand { get; }

        public ConsumptionObjectListViewModel()
        {
            _repository = new ConsumptionObjectRepository();

            Items = new ObservableCollection<ConsumptionObjectDto>();
            FilteredItems = new ObservableCollection<ConsumptionObjectDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddCommand = new RelayCommand(_ => AddObject());
            EditCommand = new RelayCommand(_ => EditObject(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteObject(), _ => SelectedItem != null);
            ShowMetersCommand = new RelayCommand<ConsumptionObjectDto>(obj => ShowMeters(obj));

            System.Diagnostics.Debug.WriteLine("ConsumptionObjectListViewModel конструктор");

            // Загружаем данные
            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                System.Diagnostics.Debug.WriteLine("LoadDataAsync START");

                var list = await _repository.GetAllAsync();

                System.Diagnostics.Debug.WriteLine($"Получено {list.Count} объектов");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Items.Clear();
                    foreach (var obj in list)
                    {
                        Items.Add(obj);
                    }

                    ApplyFilter();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка LoadDataAsync: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки объектов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            FilteredItems.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Items
                : new ObservableCollection<ConsumptionObjectDto>(
                    Items.Where(o => o.Address.ToLower().Contains(SearchText.ToLower())));

            foreach (var obj in filtered)
            {
                FilteredItems.Add(obj);
            }

            System.Diagnostics.Debug.WriteLine($"ApplyFilter: {FilteredItems.Count} объектов после фильтра");
        }


        private void AddObject()
        {
            var editViewModel = new ConsumptionObjectEditViewModel();
            var editView = new Views.Objects.ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnObjectSaved += async (s, e) =>
            {
                await LoadDataAsync();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void EditObject()
        {
            if (SelectedItem == null)
            {
                System.Diagnostics.Debug.WriteLine("EditObject: SelectedItem == null, выход");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"EditObject: начинаем редактирование объекта ID={SelectedItem.Id}, Address={SelectedItem.Address}");

            try
            {
                var editViewModel = new ConsumptionObjectEditViewModel(SelectedItem);

                if (editViewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditObject: editViewModel == null");
                    return;
                }

                var editView = new Views.Objects.ConsumptionObjectEditView(editViewModel);

                if (editView == null)
                {
                    System.Diagnostics.Debug.WriteLine("EditObject: editView == null");
                    return;
                }

                editView.Owner = Application.Current.MainWindow;

                editViewModel.OnObjectSaved += async (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("EditObject: OnObjectSaved сработал");
                    await LoadDataAsync();
                    editView.Close();
                };

                editView.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EditObject: ОШИБКА - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowMeters(ConsumptionObjectDto obj)
        {
            if (obj == null) return;

            var window = new Views.Meters.MetersForObjectView();
            var viewModel = new ViewModels.Meters.MetersForObjectViewModel(obj);
            window.DataContext = viewModel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private async void DeleteObject()
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show(
                $"Удалить объект \"{SelectedItem.Address}\"?\n\nВсе связанные данные также будут удалены!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedItem.Id);
                    await LoadDataAsync();
                    MessageBox.Show("Объект удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}