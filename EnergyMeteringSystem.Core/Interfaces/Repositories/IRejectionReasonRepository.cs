using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IRejectionReasonRepository
    {
        List<RejectionReasonDto> GetAll();
    }
}
