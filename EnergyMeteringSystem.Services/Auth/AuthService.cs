using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Interfaces.Services;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

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