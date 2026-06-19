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
        public async Task<List<UserDto>> GetAllAsync()
        {
            try
            {
                var users = await _context.User
                    .Include(u => u.UserRole)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FullName = u.FullName,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        RoleName = u.UserRole.Name,
                        IsActive = u.IsActive,
                        CreatedAt = u.CreatedAt
                    })
                    .OrderBy(u => u.FullName)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UserRepository.GetAllAsync: {ex.Message}");
                return new List<UserDto>();
            }
        }

        public List<UserDto> GetAll()
        {
            return GetAllAsync().Result;
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            try
            {
                var user = await _context.User
                    .Include(u => u.UserRole)
                    .FirstOrDefaultAsync(u => u.Id == id);

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UserRepository.GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public UserDto GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public async Task<UserDto> GetByUsernameAsync(string username)
        {
            try
            {
                var user = await _context.User
                    .Include(u => u.UserRole)
                    .FirstOrDefaultAsync(u => u.Username == username);

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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in UserRepository.GetByUsernameAsync: {ex.Message}");
                return null;
            }
        }

        public UserDto GetByUsername(string username)
        {
            return GetByUsernameAsync(username).Result;
        }

        public async Task<int> AddAsync(UserDto dto)
        {
            var entity = new User
            {
                Username = dto.Username,
                PasswordHash = dto.PasswordHash ?? PasswordHelper.HashPassword("12345"),
                FullName = dto.FullName,
                Email = dto.Email,
                RoleId = dto.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.User.Add(entity);
            await SaveChangesAsync();

            AuditLogger.Log("INSERT", "User", entity.Id, null, new { dto.Username, dto.FullName, dto.Email });

            return entity.Id;
        }

        public void Add(UserDto dto)
        {
            AddAsync(dto).Wait();
        }

        public async Task<bool> UpdateAsync(UserDto dto)
        {
            var entity = await _context.User.FindAsync(dto.Id);
            if (entity == null) return false;

            var oldValues = new { entity.FullName, entity.Email, entity.RoleId };
            var newValues = new { dto.FullName, dto.Email, dto.RoleId };

            entity.FullName = dto.FullName;
            entity.Email = dto.Email;
            entity.RoleId = dto.RoleId;

            await SaveChangesAsync();

            AuditLogger.Log("UPDATE", "User", entity.Id, oldValues, newValues);

            return true;
        }

        public void Update(UserDto dto)
        {
            UpdateAsync(dto).Wait();
        }

        public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
        {
            var entity = await _context.User.FindAsync(id);
            if (entity == null) return false;

            var oldValues = new { entity.IsActive };
            var newValues = new { IsActive = isActive };

            entity.IsActive = isActive;

            await SaveChangesAsync();

            AuditLogger.Log("UPDATE", "User", id, oldValues, newValues);

            return true;
        }

        public void SetActiveStatus(int id, bool isActive)
        {
            SetActiveStatusAsync(id, isActive).Wait();
        }

        public async Task<bool> ResetPasswordAsync(int id, string newPasswordHash)
        {
            var entity = await _context.User.FindAsync(id);
            if (entity == null) return false;

            var oldValues = new { entity.PasswordHash };
            var newValues = new { PasswordHash = newPasswordHash };

            entity.PasswordHash = newPasswordHash;

            await SaveChangesAsync();

            AuditLogger.Log("UPDATE", "User", id, oldValues, newValues);

            return true;
        }

        public void ResetPassword(int id, string newPasswordHash)
        {
            ResetPasswordAsync(id, newPasswordHash).Wait();
        }

        public async Task<List<UserRoleDto>> GetAllRolesAsync()
        {
            return await _context.UserRole
                .Select(r => new UserRoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                })
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public List<UserRoleDto> GetAllRoles()
        {
            return GetAllRolesAsync().Result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.User.FindAsync(id);
            if (entity == null) return false;

            bool hasAuditLogs = await _context.AuditLog.AnyAsync(a => a.UserId == id);
            bool hasMeterReadings = await _context.MeterReading.AnyAsync(r => r.EnteredByUserId == id);

            if (hasAuditLogs || hasMeterReadings)
            {
                throw new InvalidOperationException("������ ������� ������������, � �������� ���� ��������� ������");
            }

            var oldValues = new { entity.Username, entity.FullName };

            _context.User.Remove(entity);
            await SaveChangesAsync();

            AuditLogger.Log("DELETE", "User", id, oldValues, null);

            return true;
        }

        public void Delete(int id)
        {
            DeleteAsync(id).Wait();
        }
    }
}