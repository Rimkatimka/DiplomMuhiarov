using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class TariffTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ZoneCount { get; set; }
        public string Description { get; set; }
    }
}
