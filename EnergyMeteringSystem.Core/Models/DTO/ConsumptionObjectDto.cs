using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Core.Models.DTO
{
    public class ConsumptionObjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; } // можно склеить улицу + дом + кв
        public string ObjectTypeName { get; set; }
        public decimal? TotalArea { get; set; }
        public int? ResidentCount { get; set; }
    }
}
