using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Services.DynamicForms.Services
{
    public interface IDynamicRepository
    {
        Task<DataTable> GetAllAsDataTableAsync(string tableName);
        Task<List<Dictionary<string, object>>> GetAllAsync(string tableName);
        Task<Dictionary<string, object>> GetByIdAsync(string tableName, int id);
        Task<int> InsertAsync(string tableName, Dictionary<string, object> values);
        Task<bool> UpdateAsync(string tableName, int id, Dictionary<string, object> values);
        Task<bool> DeleteAsync(string tableName, int id);
        Task<List<ComboBoxItem>> GetComboBoxDataAsync(string tableName);
        Task<bool> TableExistsAsync(string tableName);
    }
}