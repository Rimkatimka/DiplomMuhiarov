using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Core.Helpers;
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

        public List<UserDto> GetAll()
        {
            try
            {
                var users = _context.User
                    .Include("UserRole")
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"UserRepository: loaded {users.Count} users");

                return users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.UserRole?.Name,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UserRepository.GetAll: {ex.Message}");
                return new List<UserDto>();
            }
        }

        public UserDto GetById(int id)
        {
            var u = _context.User
                .Include(u => u.UserRole)
                .FirstOrDefault(x => x.Id == id);

            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.UserRole?.Name,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            };
        }

        public UserDto GetByUsername(string username)
        {
            System.Diagnostics.Debug.WriteLine($"Searching for user: {username}");

            var user = _context.User
                .Include("UserRole")
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"User {username} not found in database");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Found user: ID={user.Id}, RoleId={user.RoleId}");

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                PasswordHash = user.PasswordHash,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.UserRole?.Name,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public void Add(UserDto dto)
        {
            var entity = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordHelper.HashPassword("12345"), // Пароль по умолчанию
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = dto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.User.Add(entity);
            _context.SaveChanges();
        }

        public void Update(UserDto dto)
        {
            var entity = _context.User.Find(dto.Id);
            if (entity != null)
            {
                entity.FullName = dto.FullName;
                entity.Email = dto.Email;
                entity.RoleId = dto.RoleId;
                _context.SaveChanges();
            }
        }

        public void SetActiveStatus(int id, bool isActive)
        {
            var entity = _context.User.Find(id);
            if (entity != null)
            {
                entity.IsActive = isActive;
                _context.SaveChanges();
            }
        }

        public void ResetPassword(int id, string newPasswordHash)
        {
            var entity = _context.User.Find(id);
            if (entity != null)
            {
                entity.PasswordHash = newPasswordHash;
                _context.SaveChanges();
            }
        }

        public List<UserRoleDto> GetAllRoles()
        {
            return _context.UserRole
                .Select(r => new UserRoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                })
                .ToList();
        }
    }
}