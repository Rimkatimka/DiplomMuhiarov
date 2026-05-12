using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IMeterTypeRepository
    {
        List<MeterTypeDto> GetAll();
        MeterTypeDto GetById(int id);
    }
}