using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        List<UserDto> GetAll();
        UserDto GetById(int id);
        UserDto GetByUsername(string username);
        void Add(UserDto user);
        void Update(UserDto user);
        void SetActiveStatus(int id, bool isActive);
        void ResetPassword(int id, string newPasswordHash);
        List<UserRoleDto> GetAllRoles();
    }
}
