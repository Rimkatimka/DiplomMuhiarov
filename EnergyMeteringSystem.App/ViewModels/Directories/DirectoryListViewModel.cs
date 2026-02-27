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
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
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
            System.Diagnostics.Debug.WriteLine("DirectoryListViewModel constructor START");

            if (repository == null)
            {
                System.Diagnostics.Debug.WriteLine("ОШИБКА: repository = null");
                throw new ArgumentNullException(nameof(repository), "Репозиторий не может быть null");
            }

            _repository = repository;
            Items = [];
            FilteredItems = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddItem());
            EditCommand = new RelayCommand(_ => EditItem(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteItem(), _ => SelectedItem != null);

            System.Diagnostics.Debug.WriteLine("DirectoryListViewModel constructor calling LoadData");
            LoadData();
            System.Diagnostics.Debug.WriteLine("DirectoryListViewModel constructor END");
        }

        private void LoadData()
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

        private void ApplyFilter()
        {
            FilteredItems.Clear();

            ObservableCollection<DirectoryDto> filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Items
                : [.. Items.Where(i =>
                        i.Name.Contains(SearchText) ||
                        (i.Description?.Contains(SearchText) ?? false))];

            foreach (DirectoryDto item in filtered)
            {
                FilteredItems.Add(item);
            }
        }

        private void AddItem()
        {
            DirectoryEditViewModel editViewModel = new();
            Views.Directories.DirectoryEditView editView = new(editViewModel);

            editViewModel.OnDirectorySaved += (s, e) =>
            {
                // Здесь нужно добавить сохранение через репозиторий
                DirectoryDto dto = new()
                {
                    Name = editViewModel.Name,
                    Description = editViewModel.Description,
                    IsActive = true
                };
                _repository.Add(dto);

                LoadData();
            };

            _ = editView.ShowDialog();
        }

        private void EditItem()
        {
            if (SelectedItem == null)
            {
                return;
            }

            DirectoryEditViewModel editViewModel = new(SelectedItem);
            Views.Directories.DirectoryEditView editView = new(editViewModel);

            editViewModel.OnDirectorySaved += (s, e) =>
            {
                SelectedItem.Name = editViewModel.Name;
                SelectedItem.Description = editViewModel.Description;
                _repository.Update(SelectedItem);

                LoadData();
            };

            _ = editView.ShowDialog();
        }

        private void DeleteItem()
        {
            MessageBoxResult result = MessageBox.Show("Удалить запись?", "Подтверждение",
    MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (SelectedItem != null && result == MessageBoxResult.Yes)
            {
                _repository.Delete(SelectedItem.Id);
                LoadData();
            }
        }
    }
}
