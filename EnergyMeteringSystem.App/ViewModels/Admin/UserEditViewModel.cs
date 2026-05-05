using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class UserEditViewModel : ViewModelBase
    {
        private readonly UserRepository _userRepository;
        private UserDto _user;

        public event EventHandler OnUserSaved;

        public ObservableCollection<UserRoleDto> Roles { get; set; }

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                SetProperty(ref _fullName, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                // Не проверяем здесь, чтобы не раздражать пользователя
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private string _emailError;
        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        private bool _showEmailError;
        public bool ShowEmailError
        {
            get => _showEmailError;
            set => SetProperty(ref _showEmailError, value);
        }

        public UserRoleDto SelectedRole { get; set; }

        public bool IsEditMode { get; private set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public UserEditViewModel(ObservableCollection<UserRoleDto> roles, UserDto existingUser = null)
        {
            _userRepository = new UserRepository();
            Roles = roles;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            if (existingUser != null)
            {
                IsEditMode = true;
                LoadUser(existingUser);
            }
            else
            {
                IsEditMode = false;
                Username = string.Empty;
                FullName = string.Empty;
                Email = string.Empty;
            }

            ShowEmailError = false;
            EmailError = string.Empty;
        }

        private void LoadUser(UserDto user)
        {
            _user = user;
            Username = user.Username;
            FullName = user.FullName;
            Email = user.Email;
            SelectedRole = FindRole(user.RoleId);
        }

        private UserRoleDto FindRole(int id)
        {
            foreach (var role in Roles)
                if (role.Id == id) return role;
            return null;
        }

        /// <summary>
        /// Проверка email - вызывается при потере фокуса
        /// </summary>
        public void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email обязателен для заполнения";
                ShowEmailError = true;
                return;
            }

            // Проверка формата email
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            bool isValid = Regex.IsMatch(Email, emailPattern);

            if (!isValid)
            {
                ShowEmailError = true;
            }
            else
            {
                EmailError = string.Empty;
                ShowEmailError = false;
            }

            SaveCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Скрыть сообщение об ошибке (при фокусе)
        /// </summary>
        public void HideEmailError()
        {
            ShowEmailError = false;
        }

        private bool CanSave()
        {
            // Базовая проверка на заполненность
            bool hasRequiredFields = !string.IsNullOrWhiteSpace(Username) &&
                                     !string.IsNullOrWhiteSpace(FullName) &&
                                     !string.IsNullOrWhiteSpace(Email) &&
                                     SelectedRole != null;

            if (!hasRequiredFields)
                return false;

            // Проверка формата email (без показа ошибки)
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(Email, emailPattern);
        }

        private void Save()
        {
            // Финальная проверка перед сохранением
            ValidateEmail();

            if (ShowEmailError)
            {
                System.Windows.MessageBox.Show("Исправьте ошибки в форме", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var dto = new UserDto
            {
                Id = _user?.Id ?? 0,
                Username = Username,
                FullName = FullName,
                Email = Email,
                RoleId = SelectedRole.Id
            };

            if (IsEditMode)
                _userRepository.Update(dto);
            else
                _userRepository.Add(dto);

            OnUserSaved?.Invoke(this, EventArgs.Empty);
        }

        private void Cancel()
        {
            OnUserSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}