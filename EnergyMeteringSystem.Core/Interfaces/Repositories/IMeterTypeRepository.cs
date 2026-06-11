using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IMeterTypeRepository
    {
        List<MeterTypeDto> GetAll();
        MeterTypeDto GetById(int id);

        Task<List<MeterTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<MeterTypeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}