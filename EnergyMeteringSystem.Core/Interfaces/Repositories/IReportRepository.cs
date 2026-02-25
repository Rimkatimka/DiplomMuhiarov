using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IReportRepository
    {
        List<ConsumptionReportDto> GetConsumptionReport(DateTime startDate, DateTime endDate);
        List<AccrualReportDto> GetAccrualReport(int year, int month);
        List<DebtDto> GetDebtReport();
    }
}
