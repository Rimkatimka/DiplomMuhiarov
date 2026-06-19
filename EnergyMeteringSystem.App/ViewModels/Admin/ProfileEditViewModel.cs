using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class ProfileEditViewModel : EditViewModelBase<UserDto, UserRepository>
    {
        private string _username;
        private string _fullName;
        private string _email;
        private string _password;
        private string _confirmPassword;

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

        public ProfileEditViewModel(UserDto currentUser) : base(new UserRepository(), currentUser)
        {
            Title = "Редактирование профиля";  // ✅ Теперь работает
        }

        protected override void LoadItem(UserDto item)
        {
            Username = item.Username;
            FullName = item.FullName;
            Email = item.Email;
        }

        protected override UserDto GetDto()
        {
            var dto = new UserDto
            {
                Id = _originalItem.Id,
                Username = Username,
                FullName = FullName,
                Email = Email,
                RoleId = _originalItem.RoleId,
                IsActive = _originalItem.IsActive
            };

            if (!string.IsNullOrEmpty(Password))
            {
                dto.PasswordHash = Core.Helpers.PasswordHelper.HashPassword(Password);
            }

            return dto;
        }

        protected override async Task<bool> SaveToRepositoryAsync(UserDto dto)
        {
            await _repository.UpdateAsync(dto);

            if (!string.IsNullOrEmpty(Password))
            {
                await _repository.ResetPasswordAsync(dto.Id, Core.Helpers.PasswordHelper.HashPassword(Password));
            }

            return true;
        }

        protected override bool CanSave()
        {
            if (string.IsNullOrWhiteSpace(Username)) return false;
            if (string.IsNullOrWhiteSpace(Email)) return false;

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(Email, emailPattern)) return false;

            if (!string.IsNullOrEmpty(Password))
            {
                if (Password.Length < 3) return false;
                if (Password != ConfirmPassword) return false;
            }

            return true;
        }
    }
}