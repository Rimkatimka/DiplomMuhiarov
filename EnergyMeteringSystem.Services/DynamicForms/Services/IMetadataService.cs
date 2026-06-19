using EnergyMeteringSystem.Services.DynamicForms.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Services.DynamicForms.Services
{
    public interface IMetadataService
    {
        Task<TableMetadata> GetTableMetadataAsync(string tableName);
        Task<List<string>> GetAllTableNamesAsync();
        void ClearCache();
    }
}