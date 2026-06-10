using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public List<UserDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            try
            {
                var users = await Query<User>()
                    .Include(u => u.UserRole)
                    .ToListAsync();

                var result = new List<UserDto>();

                foreach (var u in users)
                {
                    var lastLogin = await Query<AuditLog>()
                        .Where(a => a.UserId == u.Id && a.ActionType == "LOGIN")
                        .OrderByDescending(a => a.ActionTime)
                        .Select(a => (DateTime?)a.ActionTime)
                        .FirstOrDefaultAsync();

                    result.Add(new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FullName = u.FullName,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        RoleName = u.UserRole?.Name,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt,
                        LastLoginText = lastLogin.HasValue ? lastLogin.Value.ToString("dd.MM.yyyy HH:mm") : "Никогда"
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                return [];
            }
        }

        public UserDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var u = await Query<User>()
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(x => x.Id == id);

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
            return GetByUsernameAsync(username).Result;
        }

        public async Task<UserDto> GetByUsernameAsync(string username)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsernameAsync: поиск пользователя '{username}'");

                var user = await Query<User>()
                    .Include(u => u.UserRole)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsernameAsync: пользователь '{username}' не найден");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsernameAsync: пользователь найден: ID={user.Id}, RoleId={user.RoleId}, IsActive={user.IsActive}");

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
                System.Diagnostics.Debug.WriteLine($"UserRepository.GetByUsernameAsync ошибка: {ex.Message}");
                return null;
            }
        }

        public bool IsUsernameExists(string username, int? excludeUserId = null)
        {
            return IsUsernameExistsAsync(username, excludeUserId).Result;
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null)
        {
            try
            {
                var query = Query<User>().Where(u => u.Username == username);

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.Id != excludeUserId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в IsUsernameExistsAsync: {ex.Message}");
                return false;
            }
        }

        public void ResetPassword(int id, string newPasswordHash)
        {
            ResetPasswordAsync(id, newPasswordHash).Wait();
        }

        public async Task ResetPasswordAsync(int id, string newPasswordHash)
        {
            var entity = await _context.User.FindAsync(id);
            if (entity != null)
            {
                var oldValues = new { PasswordHash = "***" };
                var newValues = new { PasswordHash = "***" };

                entity.PasswordHash = newPasswordHash;
                await _context.SaveChangesAsync();

                AuditLogger.Log("UPDATE", "User", id, oldValues, newValues);
            }
        }

        public List<UserRoleDto> GetAllRoles()
        {
            return GetAllRolesAsync().Result;
        }

        public async Task<List<UserRoleDto>> GetAllRolesAsync()
        {
            return await Query<UserRole>()
                .Select(r => new UserRoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                })
                .ToListAsync();
        }

        public void Add(UserDto dto)
        {
            var entity = new User
            {
                Username = dto.Username,
                PasswordHash = PasswordHelper.HashPassword("12345"),
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = dto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.User.Add(entity);
            _context.SaveChanges();

            AuditLogger.Log("INSERT", "User", entity.Id, null, new { dto.Username, dto.FullName, dto.Email });
        }

        public void Update(UserDto dto)
        {
            var entity = _context.User.Find(dto.Id);
            if (entity != null)
            {
                var oldValues = new { entity.FullName, entity.Email, entity.RoleId };
                var newValues = new { dto.FullName, dto.Email, dto.RoleId };

                entity.FullName = dto.FullName;
                entity.Email = dto.Email;
                entity.RoleId = dto.RoleId;
                _context.SaveChanges();

                AuditLogger.Log("UPDATE", "User", entity.Id, oldValues, newValues);
            }
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.User.FindAsync(id);
                if (entity == null)
                {
                    throw new Exception("Пользователь не найден");
                }

                if (entity.RoleId == 3)
                {
                    throw new InvalidOperationException("Нельзя удалить администратора");
                }

                var currentUserId = GetCurrentUserId();
                if (currentUserId == id)
                {
                    throw new InvalidOperationException("Нельзя удалить свою учетную запись");
                }

                var oldValues = new { entity.Username, entity.FullName, entity.Email, entity.RoleId };

                _context.User.Remove(entity);
                await _context.SaveChangesAsync();

                AuditLogger.Log("DELETE", "User", id, oldValues, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления: {ex.Message}");
                throw;
            }
        }

        public void SetActiveStatus(int id, bool isActive)
        {
            SetActiveStatusAsync(id, isActive).Wait();
        }

        public async Task SetActiveStatusAsync(int id, bool isActive)
        {
            var entity = await _context.User.FindAsync(id);
            if (entity != null)
            {
                var oldValues = new { entity.IsActive };
                var newValues = new { IsActive = isActive };

                entity.IsActive = isActive;
                await _context.SaveChangesAsync();

                AuditLogger.Log("UPDATE", "User", entity.Id, oldValues, newValues);
            }
        }

        private int GetCurrentUserId()
        {
            return 1;
        }
    }
}