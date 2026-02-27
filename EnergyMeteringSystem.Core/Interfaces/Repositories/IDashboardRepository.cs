using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        DashboardDto GetDashboardData();
    }
}
