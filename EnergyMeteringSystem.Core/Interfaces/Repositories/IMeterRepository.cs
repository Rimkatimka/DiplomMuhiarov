using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IMeterRepository
    {
        List<MeterDto> GetByObjectId(int objectId);
    }
}
