// EnergyMeteringSystem.Services/DynamicForms/Services/MetadataService.cs
using Dapper;
using EnergyMeteringSystem.Services.DynamicForms.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnergyMeteringSystem.Services.DynamicForms.Services
{
    public class MetadataService : IMetadataService
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, TableMetadata> _cache = new();
        private readonly object _lock = new();

        // ⬇️⬇️⬇️ ДОБАВЬТЕ ЭТИ СЛОВАРИ ⬇️⬇️⬇️

        // Русские названия таблиц
        private static readonly Dictionary<string, string> _tableRussianNames = new()
        {
            { "Region", "Регионы" },
            { "City", "Города" },
            { "Street", "Улицы" },
            { "ObjectType", "Типы объектов" },
            { "MeterType", "Типы счетчиков" },
            { "MeterStatus", "Статусы счетчиков" },
            { "ReadingStatus", "Статусы показаний" },
            { "RejectionReason", "Причины отклонения" },
            { "EnergySource", "Источники энергии" },
            { "UserRole", "Роли пользователей" },
            { "VerificationInterval", "Интервалы поверки" }
        };

        // Русские названия столбцов
        private static readonly Dictionary<string, string> _columnRussianNames = new()
        {
            { "Name", "Название" },
            { "Code", "Код" },
            { "Description", "Описание" },
            { "ColorHex", "Цвет (HEX)" },
            { "CanAcceptReadings", "Можно вводить показания" },
            { "RequiresComment", "Требуется комментарий" },
            { "Voltage", "Напряжение (В)" },
            { "MaxCurrent", "Макс. ток (А)" },
            { "AccuracyClass", "Класс точности" },
            { "DigitCount", "Количество разрядов" },
            { "DecimalPlaces", "Знаков после запятой" },
            { "ServiceLifeYears", "Срок службы (лет)" },
            { "Years", "Интервал (лет)" },
            { "CapacityMW", "Мощность (МВт)" },
            { "PermissionsJson", "Права доступа" },
            { "NormConsumption", "Норматив потребления" },
            { "PostalCode", "Почтовый индекс" },
            { "RegionId", "Регион" },
            { "CityId", "Город" },
            { "StreetId", "Улица" },
            { "ObjectTypeId", "Тип объекта" },
            { "MeterTypeId", "Тип счетчика" },
            { "MeterStatusId", "Статус счетчика" },
            { "ReadingStatusId", "Статус показания" },
            { "RejectionReasonId", "Причина отклонения" },
            { "EnergySourceId", "Источник энергии" },
            { "UserRoleId", "Роль пользователя" }
        };

        // ⬆️⬆️⬆️ КОНЕЦ ДОБАВЛЕННЫХ СЛОВАРЕЙ ⬆️⬆️⬆️

        public MetadataService()
        {
            var efConnectionString = ConfigurationManager.ConnectionStrings["EnergyMeteringSystemEntities"]?.ConnectionString;

            if (string.IsNullOrEmpty(efConnectionString))
            {
                throw new InvalidOperationException("Строка подключения 'EnergyMeteringSystemEntities' не найдена в конфигурации");
            }

            _connectionString = ExtractSqlConnectionString(efConnectionString);
        }

        private string ExtractSqlConnectionString(string efConnectionString)
        {
            var match = Regex.Match(efConnectionString, "provider connection string=\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : efConnectionString;
        }

        public async Task<TableMetadata> GetTableMetadataAsync(string tableName)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(tableName, out var cached))
                    return cached;
            }

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Исправленный SQL-запрос
            string sql = @"
        SELECT 
            c.name AS ColumnName,
            ty.name AS DataType,
            c.max_length AS MaxLength,
            c.is_nullable AS IsNullable,
            c.is_identity AS IsIdentity,
            fk.name AS ForeignKeyName,
            OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
        FROM sys.columns c
        INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
        LEFT JOIN sys.foreign_key_columns fkc 
            ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
        LEFT JOIN sys.foreign_keys fk 
            ON fk.object_id = fkc.constraint_object_id
        WHERE c.object_id = OBJECT_ID(@tableName, 'U')
        ORDER BY c.column_id";

            var columnsData = (await connection.QueryAsync(sql, new { tableName })).ToList();

            // Если таблица не найдена или нет колонок – выбрасываем исключение с понятным сообщением
            if (!columnsData.Any())
                throw new InvalidOperationException($"Таблица '{tableName}' не найдена или не содержит колонок.");

            var metadata = new TableMetadata
            {
                TableName = tableName,
                RussianName = _tableRussianNames.ContainsKey(tableName)
                    ? _tableRussianNames[tableName]
                    : tableName,
                Columns = columnsData
                    .GroupBy(c => (string)c.ColumnName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .Select(c => new ColumnMetadata
                    {
                        ColumnName = c.ColumnName,
                        DataType = c.DataType,
                        MaxLength = c.MaxLength == -1 ? null : (int?)c.MaxLength,
                        IsNullable = c.IsNullable,
                        IsIdentity = c.IsIdentity,
                        IsForeignKey = !string.IsNullOrEmpty(c.ForeignKeyName),
                        ReferencedTable = c.ReferencedTable,
                        RussianName = _columnRussianNames.ContainsKey(c.ColumnName)
                            ? _columnRussianNames[c.ColumnName]
                            : c.ColumnName,
                        ControlType = DetermineControlType(c.ColumnName, c.DataType, !string.IsNullOrEmpty(c.ForeignKeyName))
                    }).ToList()
            };

            metadata.PrimaryKey = metadata.Columns.FirstOrDefault(c => c.IsIdentity);

            lock (_lock)
            {
                _cache[tableName] = metadata;
            }

            return metadata;
        }

        private ControlType DetermineControlType(string columnName, string dataType, bool isForeignKey)
        {
            if (isForeignKey || columnName.EndsWith("Id"))
                return ControlType.ComboBox;

            return dataType switch
            {
                "bit" => ControlType.CheckBox,
                "int" => ControlType.NumericTextBox,
                "decimal" => ControlType.NumericTextBox,
                "float" => ControlType.NumericTextBox,
                "date" or "datetime" or "datetime2" => ControlType.DatePicker,
                "smalldatetime" => ControlType.DatePicker,
                _ => ControlType.TextBox
            };
        }

        public async Task<List<string>> GetAllTableNamesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT t.name
                FROM sys.tables t
                WHERE t.schema_id = SCHEMA_ID('dbo')
                  AND t.name IN (
                      'Region', 'City', 'Street', 'ObjectType',
                      'MeterType', 'MeterStatus', 'ReadingStatus', 'RejectionReason',
                      'EnergySource', 'UserRole', 'VerificationInterval'
                  )
                ORDER BY t.name";

            var result = await connection.QueryAsync<string>(sql);
            return result.ToList();
        }

        public void ClearCache() => _cache.Clear();
    }
}