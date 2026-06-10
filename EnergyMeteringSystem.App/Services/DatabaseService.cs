using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

namespace EnergyMeteringSystem.App.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            var efConnectionString = ConfigurationManager.ConnectionStrings["EnergyMeteringSystemEntities"].ConnectionString;

            // Извлекаем обычную SQL строку из EF строки подключения
            _connectionString = ExtractSqlConnectionString(efConnectionString);
        }

        private string ExtractSqlConnectionString(string efConnectionString)
        {
            // Ищем provider connection string в metadata
            var match = Regex.Match(efConnectionString, "provider connection string=\"([^\"]+)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Если не нашли - возвращаем как есть
            return efConnectionString;
        }

        public List<T> ExecuteQuery<T>(string query, params SqlParameter[] parameters) where T : new()
        {
            var result = new List<T>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        var properties = typeof(T).GetProperties();

                        while (reader.Read())
                        {
                            var item = new T();
                            foreach (var prop in properties)
                            {
                                try
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal(prop.Name)))
                                    {
                                        var value = reader[prop.Name];
                                        if (value != DBNull.Value)
                                        {
                                            var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                                            prop.SetValue(item, convertedValue);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Ошибка при чтении поля {prop.Name}: {ex.Message}");
                                }
                            }
                            result.Add(item);
                        }
                    }
                }
            }

            return result;
        }

        public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }
    }
}