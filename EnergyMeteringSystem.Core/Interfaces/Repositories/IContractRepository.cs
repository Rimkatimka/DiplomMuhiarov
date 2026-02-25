using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IContractRepository
    {
        List<ContractDto> GetAll();
        List<ContractDto> GetByObjectId(int objectId);
        ContractDto GetById(int id);
        void Add(ContractDto contract);
        void Update(ContractDto contract);
        void Terminate(int id, DateTime endDate);
    }
}
