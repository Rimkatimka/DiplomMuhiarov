using System;
using System.Collections.Generic;
using EnergyMeteringSystem.Core.Models.DTO;

namespace EnergyMeteringSystem.Core.Interfaces.Repositories
{
    public interface IAuditRepository
    {
        List<AuditLogDto> GetAll();
        List<AuditLogDto> GetByDate(DateTime from, DateTime to);
        List<AuditLogDto> GetByUser(int userId);
        List<AuditLogDto> GetByTable(string tableName);
        void Log(AuditLogDto log);
    }
}