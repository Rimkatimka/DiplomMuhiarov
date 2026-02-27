using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Services
{
    public interface IAuthService
    {
        UserDto Login(string username, string password);  // ← должен быть такой метод
        UserDto GetCurrentUser();
        void Logout();
        bool HasPermission(string permission);
        bool HasAnyPermission(params string[] permissions);
        bool HasAllPermissions(params string[] permissions);
    }
}
