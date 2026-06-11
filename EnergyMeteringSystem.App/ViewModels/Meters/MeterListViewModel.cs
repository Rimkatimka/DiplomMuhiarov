using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Meters
{
    public class MeterListViewModel : ListViewModelBase<MeterDto, MeterRepository>
    {
        private readonly MeterStatusRepository _statusRepository;
        private MeterStatusDto _selectedStatus;
        private ObservableCollection<MeterStatusDto> _statuses;

        // Свойства для обратной совместимости с XAML
        public ObservableCollection<MeterDto> FilteredMeters => FilteredItems;
        public MeterDto SelectedMeter
        {
            get => SelectedItem;
            set => SelectedItem = value;
        }
        public ObservableCollection<MeterDto> Meters => Items;

        public ObservableCollection<MeterStatusDto> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        public MeterStatusDto SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                    ApplyFilter();
            }
        }

        public AsyncRelayCommand ReplaceCommand { get; }

        public MeterListViewModel() : base(new MeterRepository())
        {
            _statusRepository = new MeterStatusRepository();
            Statuses = new ObservableCollection<MeterStatusDto>();
            ReplaceCommand = new AsyncRelayCommand(async () => await ReplaceMeterAsync(), () => SelectedItem != null);

            _ = LoadStatusesAsync();
        }

        private async Task LoadStatusesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var statusList = await _statusRepository.GetAllAsync();
                Statuses.Clear();

                Statuses.Add(new MeterStatusDto { Id = 0, Name = "Все статусы" });

                foreach (var status in statusList)
                {
                    Statuses.Add(new MeterStatusDto
                    {
                        Id = status.Id,
                        Name = status.Name,
                        Description = status.Description
                    });
                }
            }, "Ошибка загрузки статусов");
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var list = await _repository.GetAllAsync();
                Items.Clear();
                foreach (var meter in list)
                    Items.Add(meter);
                ApplyFilter();
            }, "Ошибка загрузки счетчиков");
        }

        protected override async Task AddAsync()
        {
            var editViewModel = new MeterEditViewModel();
            var editView = new Views.Meters.MeterEditView(editViewModel);
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

            var editViewModel = new MeterEditViewModel(SelectedItem);
            var editView = new Views.Meters.MeterEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnSaved += async (s, e) =>
            {
                await LoadDataAsync();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private async Task ReplaceMeterAsync()
        {
            if (SelectedItem == null) return;

            await ShowMessageAsync($"Замена счетчика {SelectedItem.SerialNumber} будет реализована в следующей версии", "Информация");
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            var result = await ShowConfirmationAsync(
                $"Удалить счетчик {SelectedItem.SerialNumber}?",
                "Подтверждение удаления");

            if (result)
            {
                await ExecuteAsync(async () =>
                {
                    await _repository.DeleteAsync(SelectedItem.Id);
                    await LoadDataAsync();
                    await ShowMessageAsync("Счетчик удален", "Успех");
                }, "Ошибка при удалении");
            }
        }

        protected override void ApplyFilter()
        {
            FilteredItems.Clear();

            var filtered = Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(m => m.SerialNumber.Contains(SearchText));
            }

            if (SelectedStatus != null && SelectedStatus.Id > 0)
            {
                filtered = filtered.Where(m => m.StatusId == SelectedStatus.Id);
            }

            foreach (var meter in filtered)
                FilteredItems.Add(meter);
        }

        protected override bool ItemMatchesSearch(MeterDto item, string searchText)
        {
            return item.SerialNumber.Contains(searchText);
        }

        private async Task ShowMessageAsync(string message, string title)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private async Task<bool> ShowConfirmationAsync(string message, string title)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            });
        }
    }
}