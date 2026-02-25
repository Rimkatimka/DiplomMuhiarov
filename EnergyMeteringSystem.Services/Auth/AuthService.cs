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
    public static class AuthService
    {
        public static UserDto CurrentUser { get; private set; }

        public static UserDto Login(string username, string password)
        {
            var repo = new UserRepository();
            var user = repo.GetByUsername(username);

            if (user != null && user.IsActive && user.PasswordHash == password)
            {
                CurrentUser = user;
                return user;
            }

            return null;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
