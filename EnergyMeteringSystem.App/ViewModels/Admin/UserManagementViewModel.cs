using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Main;
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
        private UserDto _currentUser;

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
                EditCommand?.RaiseCanExecuteChanged();
                BlockCommand?.RaiseCanExecuteChanged();
                ResetPasswordCommand?.RaiseCanExecuteChanged();
                DeleteCommand?.RaiseCanExecuteChanged();
            }
        }

        public UserDto CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand BlockCommand { get; }
        public RelayCommand ResetPasswordCommand { get; }
        public RelayCommand DeleteCommand { get; }

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
            DeleteCommand = new RelayCommand(_ => DeleteUser(), _ => CanDeleteUser());

            LoadData();
            LoadRoles();
            LoadCurrentUser();
        }

        private void LoadCurrentUser()
        {
            // Получаем текущего пользователя из ShellViewModel
            if (Application.Current.MainWindow?.DataContext is ShellViewModel shell)
            {
                CurrentUser = shell.CurrentUser;
            }
        }

        private bool CanDeleteUser()
        {
            if (SelectedUser == null) return false;
            if (CurrentUser == null) return false;
            if (!CurrentUser.IsAdmin) return false;      // Только админ может удалять
            if (SelectedUser.IsAdmin) return false;      // Нельзя удалять админа
            if (CurrentUser.Id == SelectedUser.Id) return false; // Нельзя удалять себя
            return true;
        }

        private void LoadData()
        {
            Users.Clear();
            var list = _userRepository.GetAll();
            foreach (var user in list)
            {
                Users.Add(user);
            }
            ApplyFilter();
        }

        private void LoadRoles()
        {
            Roles.Clear();
            var list = _userRepository.GetAllRoles();
            foreach (var role in list)
            {
                Roles.Add(role);
            }
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
            {
                FilteredUsers.Add(user);
            }
        }

        private void AddUser()
        {
            try
            {
                var addViewModel = new UserEditViewModel(Roles);
                var addView = new Views.Admin.UserEditView(addViewModel);

                var window = new Window
                {
                    Title = "Новый пользователь",
                    Content = addView,
                    Width = 500,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Owner = Application.Current.MainWindow
                };

                addViewModel.OnUserSaved += (s, e) =>
                {
                    LoadData();
                    window.Close();
                };

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditUser()
        {
            if (SelectedUser == null) return;

            try
            {
                var editViewModel = new UserEditViewModel(Roles, SelectedUser, CurrentUser);
                var editView = new Views.Admin.UserEditView(editViewModel);

                var window = new Window
                {
                    Title = "Редактирование пользователя",
                    Content = editView,
                    Width = 500,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Owner = Application.Current.MainWindow
                };

                editViewModel.OnUserSaved += (s, e) =>
                {
                    LoadData();
                    window.Close();
                };

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ МЕТОД BlockUser
        private void BlockUser()
        {
            if (SelectedUser == null) return;

            // Нельзя заблокировать себя
            if (CurrentUser != null && CurrentUser.Id == SelectedUser.Id)
            {
                MessageBox.Show("Нельзя заблокировать свою учетную запись", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool newStatus = !SelectedUser.IsActive;
            string action = newStatus ? "Разблокировать" : "Заблокировать";
            string message = $"{action} пользователя {SelectedUser.FullName}?";

            var result = MessageBox.Show(message, "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _userRepository.SetActiveStatus(SelectedUser.Id, newStatus);
                LoadData();

                MessageBox.Show($"Пользователь {action}н", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
                string newHash = PasswordHelper.HashPassword("12345");
                _userRepository.ResetPassword(SelectedUser.Id, newHash);
                MessageBox.Show("Пароль сброшен на 12345", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ✅ МЕТОД DeleteUser
        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Удалить пользователя {SelectedUser.FullName}?\n\nВНИМАНИЕ: Это действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _userRepository.Delete(SelectedUser.Id);
                    LoadData();
                    MessageBox.Show("Пользователь удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
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