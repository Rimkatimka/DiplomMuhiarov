using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.App.ViewModels.Base;
using System.Threading.Tasks;
using System;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    /// <summary>
    /// ViewModel для справочников - наследник от ListViewModelBase
    /// </summary>
    public class DirectoryListViewModel : ListViewModelBase<DirectoryDto, IDirectoryRepository<DirectoryDto>>
    {
        private readonly string _directoryName;

        public DirectoryListViewModel(IDirectoryRepository<DirectoryDto> repository, string directoryName)
            : base(repository)
        {
            _directoryName = directoryName;
        }

        protected override async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                var items = await Task.Run(() => _repository.GetAll());
                Items.Clear();
                foreach (var item in items)
                    Items.Add(item);
                ApplyFilter();

                TotalCount = Items.Count;
                HasNextPage = false;
                HasPreviousPage = false;
            }, $"Ошибка загрузки {_directoryName}");
        }

        protected override async Task AddAsync()
        {
            var editViewModel = new DirectoryEditViewModel();
            var editView = new Views.Directories.DirectoryEditView(editViewModel);
            editView.Owner = System.Windows.Application.Current.MainWindow;

            // ✅ ИСПРАВЛЕНО: OnDirectorySaved → OnSaved
            editViewModel.OnSaved += async (s, e) =>
            {
                var dto = new DirectoryDto
                {
                    Name = editViewModel.Name,
                    Description = editViewModel.Description,
                    IsActive = true
                };

                await ExecuteAsync(async () =>
                {
                    await Task.Run(() => _repository.Add(dto));
                    await LoadDataAsync();
                    editView.Close();
                }, "Ошибка при добавлении");
            };

            editView.ShowDialog();
        }

        protected override async Task EditAsync()
        {
            if (SelectedItem == null) return;

            var editViewModel = new DirectoryEditViewModel(SelectedItem);
            var editView = new Views.Directories.DirectoryEditView(editViewModel);
            editView.Owner = System.Windows.Application.Current.MainWindow;

            // ✅ ИСПРАВЛЕНО: OnDirectorySaved → OnSaved
            editViewModel.OnSaved += async (s, e) =>
            {
                SelectedItem.Name = editViewModel.Name;
                SelectedItem.Description = editViewModel.Description;

                await ExecuteAsync(async () =>
                {
                    await Task.Run(() => _repository.Update(SelectedItem));
                    await LoadDataAsync();
                    editView.Close();
                }, "Ошибка при сохранении");
            };

            editView.ShowDialog();
        }

        protected override async Task DeleteAsync()
        {
            if (SelectedItem == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Удалить запись \"{SelectedItem.Name}\"?",
                "Подтверждение",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                await ExecuteAsync(async () =>
                {
                    await Task.Run(() => _repository.Delete(SelectedItem.Id));
                    await LoadDataAsync();
                }, "Ошибка при удалении");
            }
        }

        protected override bool ItemMatchesSearch(DirectoryDto item, string searchText)
        {
            return item.Name.Contains(searchText) ||
                   (item.Description?.Contains(searchText) ?? false);
        }
    }
}