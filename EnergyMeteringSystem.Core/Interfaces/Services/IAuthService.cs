using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Services
{
    public interface IAuthService
    {
        UserDto Login(string username, string password);
        UserDto GetCurrentUser();
        void Logout();
    }
}
