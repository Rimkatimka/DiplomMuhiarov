using EnergyMeteringSystem.Core.Helpers;  // ✅ ДОБАВИТЬ
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Interfaces.Services;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private UserDto _currentUser;

        public AuthService()
        {
            _userRepository = new UserRepository();
        }

        public UserDto Login(string username, string password)
        {
            System.Diagnostics.Debug.WriteLine($"AuthService.Login: попытка входа для '{username}'");

            UserDto user = _userRepository.GetByUsername(username);

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

            string inputHash = PasswordHelper.HashPassword(password);
            System.Diagnostics.Debug.WriteLine($"Хэш введенного пароля: '{inputHash}'");
            System.Diagnostics.Debug.WriteLine($"Хэш из БД: '{user.PasswordHash}'");

            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                System.Diagnostics.Debug.WriteLine("AuthService.Login: пароль не совпадает");
                return null;
            }

            _currentUser = user;
            System.Diagnostics.Debug.WriteLine("AuthService.Login: вход успешен");

            // ✅ ДОБАВЬТЕ ВЫЗОВ АУДИТА
            AuditLogger.Log("LOGIN", "User", user.Id, null, new { username }, user.Id);

            return user;
        }

        public UserDto GetCurrentUser()
        {
            return _currentUser;
        }

        public void Logout()
        {
            if (_currentUser != null)
            {
                // ✅ ЗАПИСЬ АУДИТА - выход из системы
                AuditLogger.Log("LOGOUT", "User", _currentUser.Id, null, new { _currentUser.Username });
                System.Diagnostics.Debug.WriteLine($"AuthService.Logout: пользователь {_currentUser.Username} вышел");
            }
            
            _currentUser = null;
        }

        public bool HasPermission(string permission)
        {
            return _currentUser != null && _currentUser.RoleId == 4;
        }

        public bool HasAnyPermission(params string[] permissions)
        {
            return _currentUser != null && _currentUser.RoleId == 4;
        }

        public bool HasAllPermissions(params string[] permissions)
        {
            return _currentUser != null && _currentUser.RoleId == 4;
        }
    }
}