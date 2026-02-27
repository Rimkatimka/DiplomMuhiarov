using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IAccrualRepository
    {
        List<AccrualDto> GetByPeriod(int year, int month);
        List<AccrualDto> GetByObjectId(int objectId);
        AccrualDto GetByObjectAndPeriod(int objectId, int year, int month);
        void Add(AccrualDto accrual);
        void Update(AccrualDto accrual);
        bool Exists(int objectId, int year, int month);
    }
}
