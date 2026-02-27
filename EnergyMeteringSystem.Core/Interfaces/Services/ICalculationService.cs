using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Services
{
    public interface ICalculationService
    {
        List<AccrualCalculationDto> CalculateForPeriod(int year, int month);
        decimal CalculateAmount(decimal consumption, int tariffId);
    }
}
