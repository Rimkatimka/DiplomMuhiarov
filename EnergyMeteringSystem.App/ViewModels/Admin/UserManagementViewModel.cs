using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class UserManagementViewModel : ViewModelBase
    {
        private readonly UserRepository _userRepository;
        private string _searchText;
        private UserDto _selectedUser;

        public ObservableCollection<UserDto> Users { get; set; }
        public ObservableCollection<UserDto> FilteredUsers { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public UserDto SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                EditCommand.RaiseCanExecuteChanged();
                BlockCommand.RaiseCanExecuteChanged();
                ResetPasswordCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand BlockCommand { get; }
        public RelayCommand ResetPasswordCommand { get; }

        public UserManagementViewModel()
        {
            _userRepository = new UserRepository();

            Users = new ObservableCollection<UserDto>();
            FilteredUsers = new ObservableCollection<UserDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddUser());
            EditCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
            BlockCommand = new RelayCommand(_ => BlockUser(), _ => SelectedUser != null);
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword(), _ => SelectedUser != null);

            LoadData();
            LoadRoles();
        }

        private void LoadData()
        {
            Users.Clear();
            var list = _userRepository.GetAll();
            System.Diagnostics.Debug.WriteLine($"UserManagement: loaded {list.Count} users");

            foreach (var user in list)
                Users.Add(user);

            ApplyFilter();
        }

        private void LoadRoles()
        {
            // Загрузка ролей если нужно
        }

        private void ApplyFilter()
        {
            FilteredUsers.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Users
                : new ObservableCollection<UserDto>(
                    Users.Where(u =>
                        u.FullName.Contains(SearchText) ||
                        u.Username.Contains(SearchText) ||
                        u.Email.Contains(SearchText)));

            foreach (var user in filtered)
                FilteredUsers.Add(user);

            System.Diagnostics.Debug.WriteLine($"ApplyFilter: {FilteredUsers.Count} users");
        }

        private void AddUser() { /* ... */ }
        private void EditUser() { /* ... */ }
        private void BlockUser() { /* ... */ }
        private void ResetPassword() { /* ... */ }
    }
}