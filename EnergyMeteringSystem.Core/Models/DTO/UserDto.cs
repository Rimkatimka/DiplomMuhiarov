using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
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
    }
}
