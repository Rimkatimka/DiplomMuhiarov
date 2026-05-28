using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.Services;
using EnergyMeteringSystem.Services.Export;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Windows;

namespace EnergyMeteringSystem.App
{
    public partial class App : Application
    {
        private const string DatabaseName = "EnergyMeteringSystem";
        private const string LocalDbInstance = "(localdb)\\MSSQLLocalDB";

        public App()
        {
            this.Exit += App_Exit;
            this.SessionEnding += App_SessionEnding;
        }

        private void App_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            CopyDatabaseBackToProject();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            CopyDatabaseBackToProject();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!AttachDatabaseIfNeeded())
            {
                Shutdown();
                return;
            }

            base.OnStartup(e);

            var exportDialogService = new ExportDialogService();
            var exportService = new ExportService(exportDialogService);

            var loginView = new Views.Auth.LoginView();
            loginView.Show();
        }

        private bool AttachDatabaseIfNeeded()
        {
            try
            {
                string workingMdfPath = DatabasePathHelper.GetWorkingDatabasePath();

                // Если рабочей базы нет - копируем из проекта данных
                if (!File.Exists(workingMdfPath))
                {
                    string sourceMdf = DatabasePathHelper.GetSourceDatabasePath();
                    if (File.Exists(sourceMdf))
                    {
                        // Создаём рабочую папку
                        string workingDir = DatabasePathHelper.GetWorkingDatabaseDirectory();
                        if (!Directory.Exists(workingDir))
                            Directory.CreateDirectory(workingDir);

                        // Копируем MDF
                        File.Copy(sourceMdf, workingMdfPath, true);

                        // Копируем LDF
                        string sourceLdf = DatabasePathHelper.GetSourceDatabaseLogPath();
                        string workingLdfPath = DatabasePathHelper.GetWorkingDatabaseLogPath();
                        if (File.Exists(sourceLdf))
                            File.Copy(sourceLdf, workingLdfPath, true);
                    }
                    else
                    {
                        MessageBox.Show($"Файл базы данных не найден:\n{sourceMdf}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                string masterConnString = $"Data Source={LocalDbInstance};Integrated Security=True;";

                // Останавливаем и запускаем LocalDB для чистой среды
                StopLocalDb();
                StartLocalDb();

                using (var connection = new SqlConnection(masterConnString))
                {
                    connection.Open();

                    // Проверяем, прикреплена ли уже база
                    string checkSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";
                    using (var cmd = new SqlCommand(checkSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@dbName", DatabaseName);
                        int exists = (int)cmd.ExecuteScalar();

                        if (exists == 0)
                        {
                            string attachSql = $@"
                                CREATE DATABASE [{DatabaseName}] ON 
                                (FILENAME = N'{workingMdfPath}')
                                FOR ATTACH;";

                            using (var attachCmd = new SqlCommand(attachSql, connection))
                            {
                                attachCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключить базу данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void CopyDatabaseBackToProject()
        {
            try
            {
                // Отключаем базу
                DetachDatabase();

                // Небольшая пауза для освобождения файлов
                Thread.Sleep(500);

                // Копируем файлы из рабочей папки в проект данных
                string sourceMdf = DatabasePathHelper.GetWorkingDatabasePath();
                string targetMdf = DatabasePathHelper.GetSourceDatabasePath();

                if (File.Exists(sourceMdf))
                {
                    // Создаём папку назначения, если её нет
                    string targetDir = Path.GetDirectoryName(targetMdf);
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    // Копируем MDF
                    File.Copy(sourceMdf, targetMdf, true);

                    // Копируем LDF
                    string sourceLdf = DatabasePathHelper.GetWorkingDatabaseLogPath();
                    string targetLdf = DatabasePathHelper.GetSourceDatabaseLogPath();

                    if (File.Exists(sourceLdf))
                        File.Copy(sourceLdf, targetLdf, true);

                    System.Diagnostics.Debug.WriteLine($"База сохранена: {targetMdf}");
                }
            }
            catch (Exception ex)
            {
                // Если не удалось через SQL, пробуем через остановку LocalDB
                TryForceCopyViaLocalDbStop();
            }
        }

        private void DetachDatabase()
        {
            string masterConnString = $"Data Source={LocalDbInstance};Integrated Security=True;";

            using (var connection = new SqlConnection(masterConnString))
            {
                connection.Open();

                // Закрываем все соединения и открепляем базу
                string detachSql = $@"
                    USE master;
                    
                    -- Принудительное закрытие всех соединений
                    DECLARE @killSql NVARCHAR(MAX) = '';
                    SELECT @killSql = @killSql + 'KILL ' + CAST(session_id AS NVARCHAR) + ';'
                    FROM sys.dm_exec_sessions
                    WHERE database_id = DB_ID('{DatabaseName}');
                    
                    IF @killSql != ''
                    BEGIN
                        EXEC sp_executesql @killSql;
                        WAITFOR DELAY '00:00:01';
                    END
                    
                    -- Отсоединение базы
                    IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{DatabaseName}')
                    BEGIN
                        EXEC sp_detach_db '{DatabaseName}';
                    END";

                using (var cmd = new SqlCommand(detachSql, connection))
                {
                    cmd.CommandTimeout = 30;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void TryForceCopyViaLocalDbStop()
        {
            try
            {
                StopLocalDb();
                Thread.Sleep(2000); // Даём время на остановку

                string sourceMdf = DatabasePathHelper.GetWorkingDatabasePath();
                string targetMdf = DatabasePathHelper.GetSourceDatabasePath();

                if (File.Exists(sourceMdf))
                {
                    string targetDir = Path.GetDirectoryName(targetMdf);
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    File.Copy(sourceMdf, targetMdf, true);

                    string sourceLdf = DatabasePathHelper.GetWorkingDatabaseLogPath();
                    string targetLdf = DatabasePathHelper.GetSourceDatabaseLogPath();

                    if (File.Exists(sourceLdf))
                        File.Copy(sourceLdf, targetLdf, true);
                }

                // Запускаем LocalDB обратно
                StartLocalDb();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить базу данных.\n\nОшибка: {ex.Message}\n\n" +
                    "Попробуйте закрыть Visual Studio и запустить приложение отдельно.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StopLocalDb()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c sqllocaldb stop MSSQLLocalDB",
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit();
            }
            catch { /* Игнорируем ошибки остановки */ }
        }

        private void StartLocalDb()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c sqllocaldb start MSSQLLocalDB",
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit();
            }
            catch { /* Игнорируем ошибки запуска */ }
        }
    }
}