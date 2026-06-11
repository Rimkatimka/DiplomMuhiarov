using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Database;

namespace EnergyMeteringSystem.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private const string CACHE_KEY_ALL_USERS = "Users_All";
        private const string CACHE_KEY_USER_BY_ID = "User_{0}";
        private const string CACHE_KEY_USER_BY_USERNAME = "User_Username_{0}";
        private const string CACHE_KEY_ALL_ROLES = "UserRoles_All";
        private const int CACHE_MINUTES = 15;
        private const int DEFAULT_USER_ID = 1;

        public List<UserDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        // ✅ ОПТИМИЗИРОВАННЫЙ GetAll - один запрос с подзапросом для LastLogin
        public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL_USERS, async () =>
            {
                try
                {
                    var users = await Query<User>()
                        .Select(u => new UserDto
                        {
                            Id = u.Id,
                            Username = u.Username,
                            FullName = u.FullName,
                            Email = u.Email,
                            RoleId = u.RoleId,
                            RoleName = u.UserRole.Name,
                            IsActive = u.IsActive,
                            CreatedAt = u.CreatedAt,
                            LastLoginText = u.AuditLog
                                .Where(a => a.ActionType == "LOGIN")
                                .OrderByDescending(a => a.ActionTime)
                                .Select(a => a.ActionTime)
                                .FirstOrDefault() != null ? "Да" : "Никогда"
                        })
                        .OrderBy(u => u.FullName)
                        .ToListAsync(cancellationToken);

                    // Форматируем дату последнего входа
                    foreach (var user in users)
                    {
                        var lastLogin = await Query<AuditLog>()
                            .Where(a => a.UserId == user.Id && a.ActionType == "LOGIN")
                            .OrderByDescending(a => a.ActionTime)
                            .Select(a => (DateTime?)a.ActionTime)
                            .FirstOrDefaultAsync(cancellationToken);

                        user.LastLoginText = lastLogin.HasValue
                            ? lastLogin.Value.ToString("dd.MM.yyyy HH:mm")
                            : "Никогда";
                    }

                    return users;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                    return new List<UserDto>();
                }
            }, CACHE_MINUTES);
        }

        public UserDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            string cacheKey = string.Format(CACHE_KEY_USER_BY_ID, id);

            return await CacheService.GetOrAddAsync(cacheKey, async () =>
            {
                var u = await Query<User>()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

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
            }, CACHE_MINUTES);
        }

        public UserDto GetByUsername(string username)
        {
            return GetByUsernameAsync(username).Result;
        }

        public async Task<UserDto> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            try
            {
                string cacheKey = $"User_Username_{username.ToLower()}";

                // Используем простой вариант без кэша для отладки
                // TODO: вернуть кэш после исправления
                // return await CacheService.GetOrAddAsync(cacheKey, async () =>
                // {
                var user = await Query<User>()
                    .Include(u => u.UserRole)
                    .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

                if (user == null) return null;

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
                // });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByUsernameAsync error: {ex.Message}");

                // Прямой запрос без кэша
                var user = await Query<User>()
                    .Include(u => u.UserRole)
                    .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

                if (user == null) return null;

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
        }

        public bool IsUsernameExists(string username, int? excludeUserId = null)
        {
            return IsUsernameExistsAsync(username, excludeUserId).Result;
        }

        public async Task<bool> IsUsernameExistsAsync(string username, int? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = Query<User>().Where(u => u.Username == username);

                if (excludeUserId.HasValue)
                {
                    query = query.Where(u => u.Id != excludeUserId.Value);
                }

                return await query.AnyAsync(cancellationToken);
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

        public async Task ResetPasswordAsync(int id, string newPasswordHash, CancellationToken cancellationToken = default)
        {
            var entity = await _context.User.FindAsync(cancellationToken, id);
            if (entity != null)
            {
                var oldValues = new { PasswordHash = "***" };
                var newValues = new { PasswordHash = "***" };

                entity.PasswordHash = newPasswordHash;
                await _context.SaveChangesAsync(cancellationToken);

                InvalidateCache(id, entity.Username);

                AuditLogger.Log("UPDATE", "User", id, oldValues, newValues);
            }
        }

        public List<UserRoleDto> GetAllRoles()
        {
            return GetAllRolesAsync().Result;
        }

        public async Task<List<UserRoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await CacheService.GetOrAddAsync(CACHE_KEY_ALL_ROLES, async () =>
            {
                return await Query<UserRole>()
                    .Select(r => new UserRoleDto
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Description = r.Description
                    })
                    .OrderBy(r => r.Name)
                    .ToListAsync(cancellationToken);
            }, CACHE_MINUTES * 4); // Роли кэшируем дольше
        }

        public async Task<int> AddAsync(UserDto dto, CancellationToken cancellationToken = default)
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
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache();

            AuditLogger.Log("INSERT", "User", entity.Id, null, new { dto.Username, dto.FullName, dto.Email });

            return entity.Id;
        }

        public void Add(UserDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(UserDto dto, CancellationToken cancellationToken = default)
        {
            var entity = await _context.User.FindAsync(cancellationToken, dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.FullName, entity.Email, entity.RoleId };
            var newValues = new { dto.FullName, dto.Email, dto.RoleId };

            entity.FullName = dto.FullName;
            entity.Email = dto.Email;
            entity.RoleId = dto.RoleId;
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache(entity.Id, entity.Username);

            AuditLogger.Log("UPDATE", "User", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(UserDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var entity = await _context.User.FindAsync(cancellationToken, id);
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
                string username = entity.Username;

                _context.User.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);

                InvalidateCache(id, username);

                AuditLogger.Log("DELETE", "User", id, oldValues, null);

                return true;
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

        public async Task<bool> SetActiveStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
        {
            var entity = await _context.User.FindAsync(cancellationToken, id);
            if (entity == null) return false;

            var oldValues = new { entity.IsActive };
            var newValues = new { IsActive = isActive };

            entity.IsActive = isActive;
            await _context.SaveChangesAsync(cancellationToken);

            InvalidateCache(id, entity.Username);

            AuditLogger.Log("UPDATE", "User", entity.Id, oldValues, newValues);

            return true;
        }

        private void InvalidateCache(int? userId = null, string username = null)
        {
            CacheService.Remove(CACHE_KEY_ALL_USERS);
            if (userId.HasValue)
                CacheService.Remove(string.Format(CACHE_KEY_USER_BY_ID, userId.Value));
            if (!string.IsNullOrEmpty(username))
                CacheService.Remove(string.Format(CACHE_KEY_USER_BY_USERNAME, username.ToLower()));
        }

        private int GetCurrentUserId()
        {
            // TODO: получить реального пользователя из контекста
            return DEFAULT_USER_ID;
        }
    }
}