using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectListViewModel : ListViewModelBase<ConsumptionObjectDto, ConsumptionObjectRepository>
    {
        public AsyncRelayCommand<ConsumptionObjectDto> ShowMetersCommand { get; }

        public ConsumptionObjectListViewModel() : base(new ConsumptionObjectRepository())
        {
            System.Diagnostics.Debug.WriteLine("ConsumptionObjectListViewModel конструктор");
            ShowMetersCommand = new AsyncRelayCommand<ConsumptionObjectDto>(async obj => await ShowMetersAsync(obj));
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine("LoadDataAsync START");

                var list = await _repository.GetAllAsync();

                System.Diagnostics.Debug.WriteLine($"Получено {list.Count} объектов");

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Очищаем и заполняем ItemsList
                    ItemsList.Clear();
                    foreach (var obj in list)
                    {
                        ItemsList.Add(obj);
                    }

                    System.Diagnostics.Debug.WriteLine($"ItemsList заполнен: {ItemsList.Count} объектов");

                    // ✅ ВАЖНО: принудительно вызываем ApplyFilter после заполнения
                    ApplyFilter();

                    System.Diagnostics.Debug.WriteLine($"После ApplyFilter: FilteredItemsList.Count = {FilteredItemsList.Count}");

                    // Принудительно обновляем UI
                    OnPropertyChanged(nameof(FilteredItemsList));
                    OnPropertyChanged(nameof(FilteredItems));
                    OnPropertyChanged(nameof(HasFilteredItems));
                });
            }, "Ошибка загрузки объектов");
        }

        protected override async Task AddAsync()
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

        protected override async Task EditAsync()
        {
            if (SelectedItem == null) return;

            var editViewModel = new ConsumptionObjectEditViewModel(SelectedItem);
            var editView = new Views.Objects.ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnObjectSaved += async (s, e) =>
            {
                await LoadDataAsync();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private async Task ShowMetersAsync(ConsumptionObjectDto obj)
        {
            if (obj == null) return;

            var window = new Views.Meters.MetersForObjectView();
            var viewModel = new ViewModels.Meters.MetersForObjectViewModel(obj);
            window.DataContext = viewModel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show(
                $"Удалить объект \"{SelectedItem.Address}\"?\n\nВсе связанные данные также будут удалены!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                await ExecuteAsync(async () =>
                {
                    await _repository.DeleteAsync(SelectedItem.Id);
                    await LoadDataAsync();
                }, "Ошибка при удалении");
            }
        }

        protected override bool ItemMatchesSearch(ConsumptionObjectDto item, string searchText)
        {
            return item.Address?.Contains(searchText) == true;
        }
    }
}