using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                SetProperty(ref _selectedItem, value);
                EditCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public DirectoryListViewModel(IDirectoryRepository<DirectoryDto> repository)
        {
            _repository = repository;
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
            Items.Clear();
            var list = _repository.GetAll();
            foreach (var item in list)
                Items.Add(item);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredItems.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Items
                : new ObservableCollection<DirectoryDto>(
                    Items.Where(i =>
                        i.Name.Contains(SearchText) ||
                        (i.Description?.Contains(SearchText) ?? false)));

            foreach (var item in filtered)
                FilteredItems.Add(item);
        }

        private void AddItem()
        {
            // Открыть окно добавления
        }

        private void EditItem()
        {
            if (SelectedItem != null)
            {
                // Открыть окно редактирования
            }
        }

        private void DeleteItem()
        {
            if (SelectedItem != null)
            {
                _repository.Delete(SelectedItem.Id);
                LoadData();
            }
        }
    }
}
