using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class UserEditViewModel : EditViewModelBase<UserDto, UserRepository>
    {
        private readonly ObservableCollection<UserRoleDto> _roles;
        private readonly UserDto _currentUser;

        private string _username;
        private string _fullName;
        private string _email;
        private string _newPassword;
        private string _confirmPassword;
        private UserRoleDto _selectedRole;

        private string _emailError;
        private bool _showEmailError;
        private string _passwordError;
        private bool _hasPasswordError;
        public bool CanEditRole => IsAdmin && !IsSelfEdit;
        public ObservableCollection<UserRoleDto> Roles => _roles;

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                    RaiseSaveCanExecuteChanged();
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                    RaiseSaveCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                    RaiseSaveCanExecuteChanged();
                }
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (SetProperty(ref _newPassword, value))
                    RaiseSaveCanExecuteChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                    RaiseSaveCanExecuteChanged();
            }
        }

        public UserRoleDto SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                    RaiseSaveCanExecuteChanged();
            }
        }

        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        public bool ShowEmailError
        {
            get => _showEmailError;
            set => SetProperty(ref _showEmailError, value);
        }

        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        public bool HasPasswordError
        {
            get => _hasPasswordError;
            set => SetProperty(ref _hasPasswordError, value);
        }

        public bool IsSelfEdit { get; set; }
        public bool IsAdmin { get; set; }
        public bool ShowRolePicker => !IsSelfEdit;

        // Конструктор для добавления
        public UserEditViewModel(ObservableCollection<UserRoleDto> roles, UserDto currentUser = null)
            : base(new UserRepository(), null)
        {
            _roles = roles ?? new ObservableCollection<UserRoleDto>();
            _currentUser = currentUser;
            IsSelfEdit = false;
            IsAdmin = currentUser?.IsAdmin ?? false;
            IsEditMode = false;
            Title = "Новый пользователь";

            Username = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            SelectedRole = _roles.Count > 0 ? _roles[0] : null;
            RaiseCanExecuteChanged();
        }

        // Конструктор для редактирования
        public UserEditViewModel(ObservableCollection<UserRoleDto> roles, UserDto existingUser, UserDto currentUser = null)
            : base(new UserRepository(), existingUser)
        {
            _roles = roles ?? new ObservableCollection<UserRoleDto>();
            _currentUser = currentUser;
            IsSelfEdit = currentUser != null && existingUser != null && currentUser.Id == existingUser.Id;
            IsAdmin = currentUser?.IsAdmin ?? false;
            IsEditMode = true;
            Title = IsSelfEdit ? "Редактирование профиля" : "Редактирование пользователя";

            SelectedRole = FindRole(existingUser.RoleId);
            RaiseCanExecuteChanged();
        }

        protected override void LoadItem(UserDto item)
        {
            Username = item.Username;
            FullName = item.FullName;
            Email = item.Email;
        }

        protected override UserDto GetDto()
        {
            return new UserDto
            {
                Id = _originalItem?.Id ?? 0,
                Username = Username,
                FullName = FullName,
                Email = Email,
                RoleId = SelectedRole?.Id ?? (_originalItem?.RoleId ?? 1)
            };
        }

        protected override async Task<bool> SaveToRepositoryAsync(UserDto dto)
        {
            if (IsEditMode)
            {
                var updated = await _repository.UpdateAsync(dto);
                if (!updated) return false;

                if (IsSelfEdit && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmPassword))
                {
                    string newHash = PasswordHelper.HashPassword(NewPassword);
                    await _repository.ResetPasswordAsync(dto.Id, newHash);
                }
            }
            else
            {
                var existing = await _repository.GetByUsernameAsync(dto.Username);
                if (existing != null)
                    throw new InvalidOperationException($"Пользователь с логином «{dto.Username}» уже существует");

                await _repository.AddAsync(dto);
            }

            return true;
        }

        protected override bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(Username)) return false;
            if (string.IsNullOrWhiteSpace(FullName)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;

            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(Email, emailPattern)) return false;

            if (!IsEditMode && SelectedRole == null) return false;

            if (IsSelfEdit)
            {
                bool hasNewPassword = !string.IsNullOrEmpty(NewPassword);
                bool hasConfirmPassword = !string.IsNullOrEmpty(ConfirmPassword);

                if (hasNewPassword != hasConfirmPassword) return false;
                if (hasNewPassword && NewPassword.Length < 3) return false;
                if (hasNewPassword && NewPassword != ConfirmPassword) return false;
            }

            return true;
        }

        protected override string GetSaveValidationMessage()
        {
            if (string.IsNullOrWhiteSpace(Username)) return "Введите логин";
            if (string.IsNullOrWhiteSpace(FullName)) return "Введите ФИО";
            if (string.IsNullOrWhiteSpace(Email)) return "Введите email";
            if (!IsEditMode && SelectedRole == null) return "Выберите роль пользователя";
            if (ShowEmailError) return EmailError;
            if (HasPasswordError) return PasswordError;
            return "Проверьте правильность введённых данных";
        }

        private void RaiseSaveCanExecuteChanged()
        {
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private UserRoleDto FindRole(int id)
        {
            foreach (var role in _roles)
                if (role.Id == id) return role;
            return null;
        }

        private void ValidateEmail()
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
        }

        public void HideEmailError()
        {
            ShowEmailError = false;
        }

        public void ValidatePasswords()
        {
            if (string.IsNullOrEmpty(NewPassword) && string.IsNullOrEmpty(ConfirmPassword))
            {
                PasswordError = string.Empty;
                HasPasswordError = false;
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                PasswordError = "Пароли не совпадают";
                HasPasswordError = true;
            }
            else if (NewPassword.Length < 3)
            {
                PasswordError = "Пароль должен содержать минимум 3 символа";
                HasPasswordError = true;
            }
            else
            {
                PasswordError = string.Empty;
                HasPasswordError = false;
            }
        }
    }
}