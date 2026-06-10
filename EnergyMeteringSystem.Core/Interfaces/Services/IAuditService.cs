using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Services
{
    public interface IAuditService
    {
        void Log(AuditLogDto log);
    }
}