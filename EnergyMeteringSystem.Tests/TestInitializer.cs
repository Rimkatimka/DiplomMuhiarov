using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyMeteringSystem.App.Helpers;
//using System.Data.SqlClient;
using System.IO;
using System;
using System.Data.SqlClient;

namespace EnergyMeteringSystem.Tests
{
    [TestClass]
    public class TestInitializer
    {
        private const string DatabaseName = "EnergyMeteringSystem";
        private const string LocalDbInstance = "(localdb)\\MSSQLLocalDB";

        [AssemblyInitialize]
        public static void InitializeDatabase(TestContext context)
        {
            System.Diagnostics.Debug.WriteLine("=== ИНИЦИАЛИЗАЦИЯ БД ДЛЯ ТЕСТОВ ===");

            try
            {
                string workingMdfPath = DatabasePathHelper.GetWorkingDatabasePath();
                System.Diagnostics.Debug.WriteLine($"Рабочий путь MDF: {workingMdfPath}");

                // Если рабочей базы нет - копируем из проекта данных
                if (!File.Exists(workingMdfPath))
                {
                    string sourceMdf = DatabasePathHelper.GetSourceDatabasePath();
                    System.Diagnostics.Debug.WriteLine($"Исходный путь MDF: {sourceMdf}");

                    if (File.Exists(sourceMdf))
                    {
                        string workingDir = DatabasePathHelper.GetWorkingDatabaseDirectory();
                        if (!Directory.Exists(workingDir))
                            Directory.CreateDirectory(workingDir);

                        File.Copy(sourceMdf, workingMdfPath, true);

                        string sourceLdf = DatabasePathHelper.GetSourceDatabaseLogPath();
                        string workingLdfPath = DatabasePathHelper.GetWorkingDatabaseLogPath();
                        if (File.Exists(sourceLdf))
                            File.Copy(sourceLdf, workingLdfPath, true);

                        System.Diagnostics.Debug.WriteLine("Файлы БД скопированы");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Исходный файл БД не найден: {sourceMdf}");
                    }
                }

                string masterConnString = $"Data Source={LocalDbInstance};Integrated Security=True;";
                System.Diagnostics.Debug.WriteLine($"Строка подключения: {masterConnString}");

                using (var connection = new SqlConnection(masterConnString))
                {
                    connection.Open();
                    System.Diagnostics.Debug.WriteLine("Подключение к master открыто");

                    string checkSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";
                    using (var cmd = new SqlCommand(checkSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@dbName", DatabaseName);
                        int exists = (int)cmd.ExecuteScalar();
                        System.Diagnostics.Debug.WriteLine($"База существует: {exists > 0}");

                        if (exists == 0)
                        {
                            string attachSql = $@"
                                CREATE DATABASE [{DatabaseName}] ON 
                                (FILENAME = N'{workingMdfPath}')
                                FOR ATTACH;";

                            using (var attachCmd = new SqlCommand(attachSql, connection))
                            {
                                attachCmd.ExecuteNonQuery();
                                System.Diagnostics.Debug.WriteLine("База данных прикреплена");
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("=== ИНИЦИАЛИЗАЦИЯ ЗАВЕРШЕНА ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ОШИБКА инициализации БД: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
            }
        }
    }
}