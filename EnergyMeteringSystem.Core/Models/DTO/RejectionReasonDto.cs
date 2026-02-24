using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class RejectionReasonDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool RequiresComment { get; set; }
    }
}
