using System;
using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IReportRepository
    {
        List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate);
        
    }
}
