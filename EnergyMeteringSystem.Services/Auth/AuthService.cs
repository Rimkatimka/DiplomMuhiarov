using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var user = _userRepository.GetByUsername(username);

            if (user == null || !user.IsActive)
                return null;

            _currentUser = user;
            return user;
        }

        public UserDto GetCurrentUser() => _currentUser;
        public void Logout() => _currentUser = null;
    }
}
