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
        public ObservableCollection<UserRoleDto> Roles { get; set; }

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
            Roles = new ObservableCollection<UserRoleDto>();

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
            foreach (var user in list)
                Users.Add(user);

            ApplyFilter();
        }

        private void LoadRoles()
        {
            Roles.Clear();
            var list = _userRepository.GetAllRoles();
            foreach (var role in list)
                Roles.Add(role);
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
        }

        private void AddUser()
        {
            var editViewModel = new UserEditViewModel(Roles);
            var editView = new Views.Admin.UserEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Новый пользователь",
                Content = editView,
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnUserSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            window.ShowDialog();
        }

        private void EditUser()
        {
            if (SelectedUser == null) return;

            var editViewModel = new UserEditViewModel(Roles, SelectedUser);
            var editView = new Views.Admin.UserEditView
            {
                DataContext = editViewModel
            };

            var window = new Window
            {
                Title = "Редактирование пользователя",
                Content = editView,
                Width = 500,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            editViewModel.OnUserSaved += (s, e) =>
            {
                LoadData();
                window.Close();
            };

            window.ShowDialog();
        }

        private void BlockUser()
        {
            if (SelectedUser == null) return;

            var newStatus = !SelectedUser.IsActive;
            var action = newStatus ? "Разблокировать" : "Заблокировать";
            var message = $"{action} пользователя {SelectedUser.FullName}?";

            var result = MessageBox.Show(message, "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _userRepository.SetActiveStatus(SelectedUser.Id, newStatus);
                LoadData();
            }
        }

        private void ResetPassword()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Сбросить пароль для {SelectedUser.FullName}?\nНовый пароль: 12345",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var newHash = PasswordHelper.HashPassword("12345");
                _userRepository.ResetPassword(SelectedUser.Id, newHash);
                MessageBox.Show("Пароль сброшен на 12345", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}