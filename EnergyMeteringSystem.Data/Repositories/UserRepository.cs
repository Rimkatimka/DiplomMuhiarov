using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly EnergyMeteringSystemEntities _context;

        public UserRepository()
        {
            _context = new EnergyMeteringSystemEntities();
        }

        public UserDto GetByUsername(string username)
        {
            var user = _context.User
                .Include("UserRole")
                .FirstOrDefault(u => u.Username == username);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.UserRole?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public UserDto GetById(int id)
        {
            var user = _context.User
                .Include("UserRole")
                .FirstOrDefault(u => u.Id == id);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.UserRole?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
