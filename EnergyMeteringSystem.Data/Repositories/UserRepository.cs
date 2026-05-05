using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;
using EnergyMeteringSystem.Data.Helpers;

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
                List<User> users = _context.User
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
                return [];
            }
        }

        public UserDto GetById(int id)
        {
            User u = _context.User
                .Include(u => u.UserRole)
                .FirstOrDefault(x => x.Id == id);

            return u == null
                ? null
                : new UserDto
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsername: поиск пользователя '{username}'");

                User user = _context.User
                    .Include("UserRole")
                    .FirstOrDefault(u => u.Username == username);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsername: пользователь '{username}' не найден");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsername: пользователь найден: ID={user.Id}, RoleId={user.RoleId}, IsActive={user.IsActive}");

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsername ошибка: {ex.Message}");
                return null;
            }
        }

        public void Add(UserDto dto)
        {
            User entity = new()
            {
                Id = IdHelper.GetNextUserId(_context),  // ← используем хелпер
                Username = dto.Username,
                PasswordHash = "12345",
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = dto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _ = _context.User.Add(entity);
            _ = _context.SaveChanges();
        }

        public bool IsUsernameExists(string username, int? excludeUserId = null)
        {
            try
            {
                IQueryable<User> query = _context.User.Where(u => u.Username == username);

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.Id != excludeUserId.Value);
                }

                return query.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в IsUsernameExists: {ex.Message}");
                return false;
            }
        }

        public void Update(UserDto dto)
        {
            User entity = _context.User.Find(dto.Id);
            if (entity != null)
            {
                entity.FullName = dto.FullName;
                entity.Email = dto.Email;
                entity.RoleId = dto.RoleId;
                _ = _context.SaveChanges();
            }
        }

        public void SetActiveStatus(int id, bool isActive)
        {
            User entity = _context.User.Find(id);
            if (entity != null)
            {
                entity.IsActive = isActive;
                _ = _context.SaveChanges();
            }
        }

        public void ResetPassword(int id, string newPasswordHash)
        {
            User entity = _context.User.Find(id);
            if (entity != null)
            {
                entity.PasswordHash = newPasswordHash;
                _ = _context.SaveChanges();
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