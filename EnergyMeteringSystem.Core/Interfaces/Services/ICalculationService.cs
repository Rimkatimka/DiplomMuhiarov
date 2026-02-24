using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Services
{
    public interface ICalculationService
    {
        List<AccrualCalculationDto> CalculateForPeriod(int year, int month);
        decimal CalculateAmount(decimal consumption, int tariffId);
    }
}
