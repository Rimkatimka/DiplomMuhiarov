using EnergyMeteringSystem.Core.Interfaces.Services;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;

namespace EnergyMeteringSystem.Services.Auth
{
    public class AuthService : IAuthService  // ← реализует интерфейс
    {
        private readonly IUserRepository _userRepository;
        private UserDto _currentUser;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        // ✅ Метод точно соответствует интерфейсу
        public UserDto Login(string username, string password)
        {
            System.Diagnostics.Debug.WriteLine($"AuthService.Login: попытка входа для '{username}'");

            var user = _userRepository.GetByUsername(username);

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine("AuthService.Login: пользователь не найден");
                return null;
            }

            if (!user.IsActive)
            {
                System.Diagnostics.Debug.WriteLine("AuthService.Login: пользователь заблокирован");
                return null;
            }

            if (user.PasswordHash != password)
            {
                System.Diagnostics.Debug.WriteLine("AuthService.Login: пароль не совпадает");
                return null;
            }

            _currentUser = user;
            System.Diagnostics.Debug.WriteLine($"AuthService.Login: пользователь сохранен");
            return user;
        }

        public UserDto GetCurrentUser()
        {
            return _currentUser;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public bool HasPermission(string permission)
        {
            if (_currentUser == null) return false;
            if (_currentUser.RoleId == 4) return true;
            return false;
        }

        public bool HasAnyPermission(params string[] permissions)
        {
            if (_currentUser == null) return false;
            if (_currentUser.RoleId == 4) return true;
            return false;
        }

        public bool HasAllPermissions(params string[] permissions)
        {
            if (_currentUser == null) return false;
            if (_currentUser.RoleId == 4) return true;
            return false;
        }
    }
}