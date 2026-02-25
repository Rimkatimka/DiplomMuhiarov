using System;
using System.Collections.ObjectModel;
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
        public bool IsUsernameReadOnly => IsEditMode;
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
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
            }
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

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   SelectedRole != null;
        }

        private void Save()
        {
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