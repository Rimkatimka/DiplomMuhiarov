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
            ShowMetersCommand = new AsyncRelayCommand<ConsumptionObjectDto>(async obj => await ShowMetersAsync(obj));
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var list = await _repository.GetAllAsync();
                Items.Clear();
                foreach (var obj in list)
                    Items.Add(obj);
                ApplyFilter();
            }, "Ошибка загрузки объектов");
        }

        protected override async Task AddAsync()
        {
            var editViewModel = new ConsumptionObjectEditViewModel();
            var editView = new Views.Objects.ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnSaved += async (s, e) =>
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

            editViewModel.OnSaved += async (s, e) =>
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
                $"Удалить объект \"{SelectedItem.Address}\"?\n\n" +
                "Все связанные данные также будут удалены!",
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
            return item.Address.Contains(searchText);
        }
    }
}