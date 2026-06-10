using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Helpers;
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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
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

        private string _passwordError;
        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        private bool _hasPasswordError;
        public bool HasPasswordError
        {
            get => _hasPasswordError;
            set => SetProperty(ref _hasPasswordError, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private UserRoleDto _selectedRole;
        public UserRoleDto SelectedRole
        {
            get => _selectedRole;
            set
            {
                SetProperty(ref _selectedRole, value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsEditMode { get; private set; }
        public bool IsSelfEdit { get; set; }
        public bool IsAdmin { get; set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        // Конструктор для добавления нового пользователя
        public UserEditViewModel(ObservableCollection<UserRoleDto> roles, UserDto currentUser = null)
        {
            System.Diagnostics.Debug.WriteLine("!!! КОНСТРУКТОР ДЛЯ ДОБАВЛЕНИЯ !!!");

            _userRepository = new UserRepository();
            Roles = roles;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            IsSelfEdit = false;
            IsAdmin = currentUser?.IsAdmin ?? false;
            IsEditMode = false;

            Username = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            SelectedRole = roles.Count > 0 ? roles[0] : null;

            ShowEmailError = false;
            EmailError = string.Empty;
            PasswordError = string.Empty;
            HasPasswordError = false;
        }

        // Конструктор для редактирования пользователя
        public UserEditViewModel(ObservableCollection<UserRoleDto> roles, UserDto existingUser, UserDto currentUser = null)
        {
            _userRepository = new UserRepository();
            Roles = roles;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            IsSelfEdit = currentUser != null && existingUser != null && currentUser.Id == existingUser.Id;
            IsAdmin = currentUser?.IsAdmin ?? false;
            IsEditMode = true;

            if (existingUser != null)
            {
                _user = existingUser;
                Username = existingUser.Username;
                FullName = existingUser.FullName;
                Email = existingUser.Email;
                SelectedRole = FindRole(existingUser.RoleId);
            }

            ShowEmailError = false;
            EmailError = string.Empty;
            PasswordError = string.Empty;
            HasPasswordError = false;
        }

        private UserRoleDto FindRole(int id)
        {
            foreach (var role in Roles)
                if (role.Id == id) return role;
            return null;
        }

        public void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email обязателен для заполнения";
                ShowEmailError = true;
                return;
            }

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            bool isValid = Regex.IsMatch(Email, emailPattern);

            if (!isValid)
            {
                EmailError = "Неверный формат email";
                ShowEmailError = true;
            }
            else
            {
                EmailError = string.Empty;
                ShowEmailError = false;
            }

            SaveCommand.RaiseCanExecuteChanged();
        }

        public void HideEmailError()
        {
            ShowEmailError = false;
        }

        private bool CanSave()
        {
            bool hasRequiredFields = !string.IsNullOrWhiteSpace(Username) &&
                                     !string.IsNullOrWhiteSpace(FullName) &&
                                     !string.IsNullOrWhiteSpace(Email);

            if (!hasRequiredFields) return false;

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            bool emailValid = Regex.IsMatch(Email, emailPattern);

            if (!emailValid) return false;

            // Проверка пароля (только для саморедактирования)
            if (IsSelfEdit)
            {
                // Если оба поля пустые - ок (пароль не меняется)
                if (string.IsNullOrEmpty(NewPassword) && string.IsNullOrEmpty(ConfirmPassword))
                {
                    // Нормальная ситуация - пароль не меняется
                }
                // Если одно поле заполнено, а другое нет
                else if (string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmPassword))
                {
                    return false;
                }
                // Если оба заполнены - проверяем совпадение и длину
                else if (NewPassword != ConfirmPassword)
                {
                    return false;
                }
                else if (NewPassword.Length < 3)
                {
                    return false;
                }
            }

            return true;
        }

        private void Save()
        {
            // Финальная проверка email
            ValidateEmail();

            if (ShowEmailError)
            {
                MessageBox.Show("Исправьте ошибки в email", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка пароля (для саморедактирования)
            if (IsSelfEdit)
            {
                if (string.IsNullOrEmpty(NewPassword) != string.IsNullOrEmpty(ConfirmPassword))
                {
                    MessageBox.Show("Заполните оба поля пароля", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword))
                {
                    if (NewPassword.Length < 3)
                    {
                        MessageBox.Show("Пароль должен содержать минимум 3 символа", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (NewPassword != ConfirmPassword)
                    {
                        MessageBox.Show("Пароли не совпадают", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            var dto = new UserDto
            {
                Id = _user?.Id ?? 0,
                Username = Username,
                FullName = FullName,
                Email = Email,
                RoleId = SelectedRole?.Id ?? (_user?.RoleId ?? 1)
            };

            try
            {
                if (IsEditMode)
                {
                    _userRepository.Update(dto);

                    if (IsSelfEdit && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword))
                    {
                        string newHash = PasswordHelper.HashPassword(NewPassword);
                        _userRepository.ResetPassword(dto.Id, newHash);
                    }

                    MessageBox.Show("Пользователь успешно обновлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _userRepository.Add(dto);
                    MessageBox.Show("Пользователь успешно добавлен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Запись аудита
                AuditLogger.Log(IsEditMode ? "UPDATE" : "INSERT", "User", dto.Id, null,
                    new { Username, FullName, Email, PasswordChanged = !string.IsNullOrEmpty(NewPassword) });

                OnUserSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            var result = MessageBox.Show("Вы уверены, что хотите отменить изменения? Все несохраненные данные будут потеряны.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OnUserSaved?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}