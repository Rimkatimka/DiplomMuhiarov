using System;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsOperator => RoleId == 1;
        public bool IsInspector => RoleId == 2;
        public bool IsAccountant => RoleId == 3;
        public bool IsAdmin => RoleId == 4;

        public string RoleText
        {
            get
            {
                switch (RoleId)
                {
                    case 1: return "Оператор";
                    case 2: return "Инспектор";
                    case 3: return "Бухгалтер";
                    case 4: return "Администратор";
                    default: return "Неизвестно";
                }
            }
        }

        public string StatusText => IsActive ? "Активен" : "Заблокирован";
        public string StatusColor => IsActive ? "Green" : "Red";
        public string DisplayName => $"{FullName} ({Username})";
        public string CreatedText => CreatedAt.ToString("dd.MM.yyyy");
    }
}