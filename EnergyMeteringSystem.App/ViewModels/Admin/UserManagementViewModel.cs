using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
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
                _ = SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public UserDto SelectedUser
        {
            get => _selectedUser;
            set
            {
                _ = SetProperty(ref _selectedUser, value);
                EditCommand?.RaiseCanExecuteChanged();
                BlockCommand?.RaiseCanExecuteChanged();
                ResetPasswordCommand?.RaiseCanExecuteChanged();
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

            Users = [];
            FilteredUsers = [];
            Roles = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddUser());  // ← добавлена команда
            EditCommand = new RelayCommand(_ => EditUser(), _ => SelectedUser != null);
            BlockCommand = new RelayCommand(_ => BlockUser(), _ => SelectedUser != null);
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword(), _ => SelectedUser != null);

            LoadData();
            LoadRoles();
        }

        private void LoadData()
        {
            Users.Clear();
            System.Collections.Generic.List<UserDto> list = _userRepository.GetAll();
            foreach (UserDto user in list)
            {
                Users.Add(user);
            }

            ApplyFilter();
        }

        private void LoadRoles()
        {
            Roles.Clear();
            System.Collections.Generic.List<UserRoleDto> list = _userRepository.GetAllRoles();
            foreach (UserRoleDto role in list)
            {
                Roles.Add(role);
            }
        }

        private void ApplyFilter()
        {
            FilteredUsers.Clear();

            ObservableCollection<UserDto> filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Users
                : [.. Users.Where(u =>
                        u.FullName.Contains(SearchText) ||
                        u.Username.Contains(SearchText) ||
                        u.Email.Contains(SearchText))];

            foreach (UserDto user in filtered)
            {
                FilteredUsers.Add(user);
            }
        }

        // ✅ Исправленный метод AddUser
        private void AddUser()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("AddUser: начало");

                UserEditViewModel editViewModel = new(Roles);
                Views.Admin.UserEditView editView = new(editViewModel);

                Window window = new()
                {
                    Title = "Новый пользователь",
                    Content = editView,
                    Width = 500,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                editViewModel.OnUserSaved += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: пользователь сохранен");
                    LoadData();  // обновить список

                    // ✅ СООБЩЕНИЕ ОБ УСПЕШНОМ ДОБАВЛЕНИИ
                    _ = MessageBox.Show("Пользователь успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    window.Close();
                };

                _ = window.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в AddUser: {ex.Message}");
                _ = MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser()
        {
            if (SelectedUser == null)
            {
                return;
            }

            try
            {
                UserEditViewModel editViewModel = new(Roles, SelectedUser);

                // ✅ Передаем ViewModel в конструктор View
                Views.Admin.UserEditView editView = new(editViewModel);

                Window window = new()
                {
                    Title = "Редактирование пользователя",
                    Content = editView,
                    Width = 500,
                    Height = 400,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                editViewModel.OnUserSaved += (s, e) =>
                {
                    LoadData();
                    window.Close();
                };

                _ = window.ShowDialog();
            }
            catch (Exception ex)
            {
                _ = System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }



        private void BlockUser()
        {
            if (SelectedUser == null)
            {
                return;
            }

            bool newStatus = !SelectedUser.IsActive;
            string action = newStatus ? "Разблокировать" : "Заблокировать";
            string message = $"{action} пользователя {SelectedUser.FullName}?";

            MessageBoxResult result = MessageBox.Show(message, "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _userRepository.SetActiveStatus(SelectedUser.Id, newStatus);
                LoadData();
            }
        }

        private void ResetPassword()
        {
            if (SelectedUser == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Сбросить пароль для {SelectedUser.FullName}?\nНовый пароль: 12345",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string newHash = PasswordHelper.HashPassword("12345");
                _userRepository.ResetPassword(SelectedUser.Id, newHash);
                _ = MessageBox.Show("Пароль сброшен на 12345", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}