// EnergyMeteringSystem.Services/DynamicForms/Services/DynamicRepository.cs
using Dapper;
using EnergyMeteringSystem.Services.DynamicForms.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;

namespace EnergyMeteringSystem.Services.DynamicForms.Services
{
    public class DynamicRepository : IDynamicRepository
    {
        private readonly string _connectionString;
        private readonly IMetadataService _metadataService;
        private readonly Dictionary<string, List<ComboBoxItem>> _comboBoxCache = new();

        public DynamicRepository(IMetadataService metadataService)
        {
            _metadataService = metadataService;
            var efConnectionString = ConfigurationManager.ConnectionStrings["EnergyMeteringSystemEntities"].ConnectionString;
            _connectionString = ExtractSqlConnectionString(efConnectionString);
        }

        private string ExtractSqlConnectionString(string efConnectionString)
        {
            var match = System.Text.RegularExpressions.Regex.Match(efConnectionString, "provider connection string=\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : efConnectionString;
        }

        public async Task<DataTable> GetAllAsDataTableAsync(string tableName)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SELECT * FROM [{tableName}] ORDER BY [Id]", connection);
            using var reader = await command.ExecuteReaderAsync();

            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        public async Task<List<Dictionary<string, object>>> GetAllAsync(string tableName)
        {
            var table = await GetAllAsDataTableAsync(tableName);
            return DataTableHelper.ToDictionaryList(table);
        }

        public async Task<Dictionary<string, object>> GetByIdAsync(string tableName, int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand($"SELECT * FROM [{tableName}] WHERE [Id] = @Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            var table = new DataTable();
            table.Load(reader);

            return DataTableHelper.ToDictionary(table.Rows.Count > 0 ? table.Rows[0] : null);
        }

        public async Task<int> InsertAsync(string tableName, Dictionary<string, object> values)
        {
            var metadata = await _metadataService.GetTableMetadataAsync(tableName);
            var columns = metadata.Columns
                .Where(c => !c.IsIdentity)
                .Where(c => values.Keys.Any(k => string.Equals(k, c.ColumnName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (columns.Count == 0)
                throw new InvalidOperationException("Не указаны данные для сохранения");

            var columnNames = string.Join(", ", columns.Select(c => $"[{c.ColumnName}]"));
            var paramNames = string.Join(", ", columns.Select(c => "@" + c.ColumnName));

            string sql = $"INSERT INTO [{tableName}] ({columnNames}) VALUES ({paramNames}); SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var id = await connection.ExecuteScalarAsync<int>(sql, values);
            return id;
        }

        public async Task<bool> UpdateAsync(string tableName, int id, Dictionary<string, object> values)
        {
            var metadata = await _metadataService.GetTableMetadataAsync(tableName);
            var columns = metadata.Columns
                .Where(c => !c.IsIdentity)
                .Where(c => values.Keys.Any(k => string.Equals(k, c.ColumnName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (columns.Count == 0)
                return false;

            var setClause = string.Join(", ", columns.Select(c => $"[{c.ColumnName}] = @{c.ColumnName}"));
            string sql = $"UPDATE [{tableName}] SET {setClause} WHERE [Id] = @Id";

            values["Id"] = id;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var rows = await connection.ExecuteAsync(sql, values);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(string tableName, int id)
        {
            string sql = $"DELETE FROM [{tableName}] WHERE Id = @Id";

            using var connection = new SqlConnection(_connectionString);
            var rows = await connection.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        public async Task<List<ComboBoxItem>> GetComboBoxDataAsync(string tableName)
        {
            // Кэшируем данные для ComboBox
            if (_comboBoxCache.TryGetValue(tableName, out var cached))
                return cached;

            using var connection = new SqlConnection(_connectionString);

            // Проверяем наличие колонки Name
            var hasName = await connection.QueryFirstOrDefaultAsync<int?>($@"
                SELECT 1 FROM sys.columns 
                WHERE object_id = OBJECT_ID(@tableName) AND name = 'Name'", new { tableName });

            string sql = hasName.HasValue
                ? $"SELECT Id, Name as DisplayName FROM [{tableName}] ORDER BY Name"
                : $"SELECT Id, CAST(Id AS NVARCHAR) as DisplayName FROM [{tableName}] ORDER BY Id";

            var result = (await connection.QueryAsync<ComboBoxItem>(sql)).ToList();

            lock (_comboBoxCache)
            {
                _comboBoxCache[tableName] = result;
            }

            return result;
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT COUNT(*) FROM sys.tables 
                WHERE schema_id = SCHEMA_ID('dbo') AND name = @tableName";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { tableName });
            return count > 0;
        }

        public void ClearComboBoxCache() => _comboBoxCache.Clear();
    }
}