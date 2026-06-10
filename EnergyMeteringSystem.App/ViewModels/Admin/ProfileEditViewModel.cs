using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class ProfileEditViewModel : ViewModelBase
    {
        private readonly UserRepository _userRepository;
        private UserDto _user;

        public event EventHandler OnProfileSaved;

        private string _username;
        private string _fullName;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ProfileEditViewModel(UserDto currentUser)
        {
            _userRepository = new UserRepository();
            _user = currentUser;

            // Загружаем текущие данные
            Username = currentUser.Username;
            FullName = currentUser.FullName;
            Email = currentUser.Email;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private bool CanSave()
        {
            // Логин не может быть пустым
            if (string.IsNullOrWhiteSpace(Username)) return false;

            // Email не может быть пустым
            if (string.IsNullOrWhiteSpace(Email)) return false;

            // Проверка формата email
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(Email, emailPattern)) return false;

            // Если пароль введен - проверяем подтверждение
            if (!string.IsNullOrEmpty(Password))
            {
                if (Password.Length < 3)
                    return false;
                if (Password != ConfirmPassword)
                    return false;
            }

            return true;
        }

        private void Save()
        {
            try
            {
                ErrorMessage = string.Empty;

                var oldValues = new
                {
                    _user.Username,
                    _user.FullName,
                    _user.Email,
                    HasPasswordChanged = false
                };

                var newValues = new
                {
                    Username,
                    FullName,
                    Email,
                    HasPasswordChanged = !string.IsNullOrEmpty(Password)
                };

                // Обновляем данные
                _user.Username = Username;
                _user.FullName = FullName;
                _user.Email = Email;

                // Обновляем пароль, если введен новый
                if (!string.IsNullOrEmpty(Password))
                {
                    _user.PasswordHash = Core.Helpers.PasswordHelper.HashPassword(Password);
                }

                _userRepository.Update(_user);

                // Запись аудита
                Core.Helpers.AuditLogger.Log("UPDATE", "User", _user.Id, oldValues, newValues, _user.Id);

                MessageBox.Show("Профиль успешно обновлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                OnProfileSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show(ErrorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            OnProfileSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}