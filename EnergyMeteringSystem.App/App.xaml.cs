using EnergyMeteringSystem.App.Services;
using EnergyMeteringSystem.Services.Export;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace EnergyMeteringSystem.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Автоматически прикрепляем базу к LocalDB
            AttachDatabaseIfNeeded();

            base.OnStartup(e);

            var exportDialogService = new ExportDialogService();
            var exportService = new ExportService(exportDialogService);

            var loginView = new Views.Auth.LoginView();
            loginView.Show();
        }
        private void AttachDatabaseIfNeeded()
        {
            try
            {
                string mdfPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Database",
                    "EnergyMeteringSystem.mdf");

                if (!File.Exists(mdfPath))
                {
                    MessageBox.Show("Файл базы данных не найден: " + mdfPath, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

                string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;";

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string attachSql = $@"
                        IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'EnergyMeteringSystem')
                        BEGIN
                            CREATE DATABASE [EnergyMeteringSystem] ON 
                            (FILENAME = N'{mdfPath}')
                            FOR ATTACH;
                        END";

                    using (var command = new SqlCommand(attachSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подключить базу данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}