using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    public class DirectoryListViewModel : ViewModelBase
    {
        private readonly IDirectoryRepository<DirectoryDto> _repository;
        private DirectoryDto _selectedItem;
        private string _searchText;

        public ObservableCollection<DirectoryDto> Items { get; set; }
        public ObservableCollection<DirectoryDto> FilteredItems { get; set; }

        public DirectoryDto SelectedItem
        {
            get => _selectedItem;
            set
            {
                _ = SetProperty(ref _selectedItem, value);
                EditCommand?.RaiseCanExecuteChanged();
                DeleteCommand?.RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _ = SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public DirectoryListViewModel(IDirectoryRepository<DirectoryDto> repository)
        {
            System.Diagnostics.Debug.WriteLine($"DirectoryListViewModel: {repository.GetType().Name}");

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Items = new ObservableCollection<DirectoryDto>();
            FilteredItems = new ObservableCollection<DirectoryDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddItem());
            EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                Items.Clear();
                List<DirectoryDto> list = _repository.GetAll();
                System.Diagnostics.Debug.WriteLine($"LoadData: got {list.Count} items");

                foreach (DirectoryDto item in list)
                {
                    Items.Add(item);
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка LoadData: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            FilteredItems.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Items
                : new ObservableCollection<DirectoryDto>(
                    Items.Where(i => i.Name.Contains(SearchText) ||
                                    (i.Description?.Contains(SearchText) ?? false)));

            foreach (DirectoryDto item in filtered)
            {
                FilteredItems.Add(item);
            }
        }

        private void AddItem()
        {
            var editViewModel = new DirectoryEditViewModel();
            var editView = new Views.Directories.DirectoryEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnDirectorySaved += (s, e) =>
            {
                var dto = new DirectoryDto
                {
                    Name = editViewModel.Name,
                    Description = editViewModel.Description,
                    IsActive = true
                };

                try
                {
                    _repository.Add(dto);
                    LoadData();
                    editView.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            editView.ShowDialog();
        }

        private void EditItem()
        {
            if (SelectedItem == null) return;

            var editViewModel = new DirectoryEditViewModel(SelectedItem);
            var editView = new Views.Directories.DirectoryEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnDirectorySaved += (s, e) =>
            {
                SelectedItem.Name = editViewModel.Name;
                SelectedItem.Description = editViewModel.Description;

                try
                {
                    _repository.Update(SelectedItem);
                    LoadData();
                    editView.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            editView.ShowDialog();
        }

        private void DeleteItem()
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show($"Удалить запись \"{SelectedItem.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.Delete(SelectedItem.Id);
                    LoadData();
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