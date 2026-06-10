using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IConsumptionObjectRepository
    {
        List<ConsumptionObjectDto> GetAll();
        ConsumptionObjectDto GetById(int id);
    }
}
