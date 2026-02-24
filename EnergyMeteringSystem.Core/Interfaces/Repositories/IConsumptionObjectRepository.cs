using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IConsumptionObjectRepository
    {
        List<ConsumptionObjectDto> GetAll();
        ConsumptionObjectDto GetById(int id);
    }
}
