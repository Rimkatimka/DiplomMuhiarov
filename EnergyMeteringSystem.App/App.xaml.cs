using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Helpers;
using EnergyMeteringSystem.App.ViewModels.Admin;
using EnergyMeteringSystem.App.ViewModels.Analytics;
using EnergyMeteringSystem.App.ViewModels.Auth;
using EnergyMeteringSystem.App.ViewModels.Directories;
using EnergyMeteringSystem.App.ViewModels.Dynamic;
using EnergyMeteringSystem.App.ViewModels.Main;
using EnergyMeteringSystem.App.ViewModels.Meters;
using EnergyMeteringSystem.App.ViewModels.Objects;
using EnergyMeteringSystem.App.ViewModels.Readings;
using EnergyMeteringSystem.App.ViewModels.Reports;
using EnergyMeteringSystem.App.Views.Admin;
using EnergyMeteringSystem.App.Views.Analytics;
using EnergyMeteringSystem.App.Views.Auth;
using EnergyMeteringSystem.App.Views.Directories;
using EnergyMeteringSystem.App.Views.Dynamic;
using EnergyMeteringSystem.App.Views.Main;
using EnergyMeteringSystem.App.Views.Meters;
using EnergyMeteringSystem.App.Views.Objects;
using EnergyMeteringSystem.App.Views.Readings;
using EnergyMeteringSystem.App.Views.Reports;
using EnergyMeteringSystem.Core.Helpers;
using EnergyMeteringSystem.Core.Interfaces.Repositories;
using EnergyMeteringSystem.Data.Repositories;
using EnergyMeteringSystem.Services.Auth;
using EnergyMeteringSystem.Services.DynamicForms.Extensions;
using EnergyMeteringSystem.Services.DynamicForms.Services;
using Microsoft.Extensions.DependencyInjection;


namespace EnergyMeteringSystem.App
{
    public partial class App : Application
    {
        private const string DatabaseName = "EnergyMeteringSystem";
        private const string LocalDbInstance = "(localdb)\\MSSQLLocalDB";

        /// <summary>
        /// DI контейнер для доступа к сервисам из любого места
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Публичный доступ к DI контейнеру
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Настройка Dependency Injection
            ConfigureServices();

            // 2. Настройка аудита
            AuditLogger.OnLog += log =>
            {
                _ = Task.Run(() =>
                {
                    try
                    {
                        using (var repository = new AuditRepository())
                        {
                            repository.Log(log);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"App: Ошибка сохранения аудита: {ex.Message}");
                    }
                });
            };

            // 3. Подключение базы данных
            if (!AttachDatabaseIfNeeded())
            {
                Shutdown();
                return;
            }

            // 4. Запуск окна входа через DI
            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            loginView.Show();
            MainWindow = loginView;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Копируем базу обратно в проект
            CopyDatabaseBackToProject();

            // Освобождаем ресурсы DI
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// Настройка Dependency Injection контейнера
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // ============================================================
            // 1. СЕРВИСЫ ДИНАМИЧЕСКИХ ФОРМ
            // ============================================================
            services.AddDynamicForms();

            // ============================================================
            // 2. РЕПОЗИТОРИИ
            // ============================================================
            services.AddTransient<ViewModels.Directories.DynamicDirectoryListViewModel>();
            services.AddTransient<Views.Directories.DynamicDirectoryView>();
            services.AddScoped<AuditRepository>();
            services.AddScoped<ConsumptionObjectRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<MeterRepository>();
            services.AddScoped<MeterReadingRepository>();
            services.AddScoped<RegionRepository>();
            services.AddScoped<CityRepository>();
            services.AddScoped<StreetRepository>();
            services.AddScoped<ObjectTypeRepository>();
            services.AddScoped<MeterTypeRepository>();
            services.AddScoped<MeterStatusRepository>();
            services.AddScoped<ReadingStatusRepository>();
            services.AddScoped<RejectionReasonRepository>();
            services.AddScoped<EnergySourceRepository>();
            services.AddScoped<VerificationIntervalRepository>();
            services.AddScoped<DashboardRepository>();
            services.AddScoped<AnalyticsRepository>();
            services.AddScoped<HierarchyAnalyticsRepository>();
            services.AddScoped<ReportRepository>();

            // ============================================================
            // 3. СЕРВИСЫ
            // ============================================================
            services.AddScoped<AuthService>();
            //services.AddScoped<Services.Export.ExportService>();

            // ============================================================
            // 4. VIEWMODELS
            // ============================================================

            // Auth
            services.AddTransient<LoginViewModel>();

            // Main
            services.AddTransient<ShellViewModel>();
            services.AddTransient<DashboardViewModel>();

            // Objects
            services.AddTransient<ConsumptionObjectListViewModel>();
            services.AddTransient<ConsumptionObjectEditViewModel>();

            // Meters
            services.AddTransient<MeterListViewModel>();
            services.AddTransient<MeterEditViewModel>();

            // Readings
            services.AddTransient<MeterReadingInputViewModel>();
            services.AddTransient<MeterReadingHistoryViewModel>();
            services.AddTransient<MeterReadingVerificationViewModel>();

            // Analytics
            services.AddTransient<AnalyticsViewModel>();
            services.AddTransient<HierarchyAnalyticsViewModel>();

            // Reports
            services.AddTransient<ReportViewModel>();

            // Admin
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<AuditLogViewModel>();
            services.AddTransient<BackupViewModel>();

            // Directories (динамические справочники)
            services.AddTransient<DynamicEditViewModel>();

            // ============================================================
            // 5. VIEWS
            // ============================================================

            // Auth
            services.AddTransient<LoginView>();

            // Main
            services.AddTransient<ShellView>();
            services.AddTransient<DashboardView>();

            // Objects
            services.AddTransient<ConsumptionObjectListView>();
            services.AddTransient<ConsumptionObjectEditView>();

            // Meters
            services.AddTransient<MeterListView>();
            services.AddTransient<MeterEditView>();

            // Readings
            services.AddTransient<MeterReadingInputView>();
            services.AddTransient<MeterReadingHistoryView>();
            services.AddTransient<MeterReadingVerificationView>();

            // Analytics
            services.AddTransient<AnalyticsView>();
            services.AddTransient<HierarchyAnalyticsView>();

            // Reports
            services.AddTransient<ReportView>();

            // Admin
            services.AddTransient<UserManagementView>();
            services.AddTransient<AuditLogView>();
            services.AddTransient<BackupView>();

            // Directories
            services.AddTransient<DirectoryListView>();

            // Dynamic Forms (универсальное окно редактора)
            services.AddTransient<DynamicEditView>();

            // ============================================================
            // 6. ПОСТРОЕНИЕ КОНТЕЙНЕРА
            // ============================================================

            _serviceProvider = services.BuildServiceProvider();

            // Проверка: все ли сервисы зарегистрированы корректно
            try
            {
                var test = _serviceProvider.GetRequiredService<IMetadataService>();
                Debug.WriteLine("DI: MetadataService зарегистрирован успешно");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DI Ошибка при регистрации: {ex.Message}");
            }
        }

        // ================================================================
        // РАБОТА С БАЗОЙ ДАННЫХ
        // ================================================================

        private bool AttachDatabaseIfNeeded()
        {
            try
            {
                string mdfPath = DatabasePathHelper.GetActiveDatabasePath();
                string ldfPath = DatabasePathHelper.GetActiveDatabaseLogPath();

                if (!File.Exists(mdfPath))
                {
                    MessageBox.Show(
                        $"Файл базы данных не найден:\n{mdfPath}\n\nСоздайте базу, выполнив script.sql из корня проекта.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                string masterConnString = $"Data Source={LocalDbInstance};Integrated Security=True;";
                StartLocalDb();

                using (var connection = new SqlConnection(masterConnString))
                {
                    connection.Open();

                    string checkSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";
                    using (var cmd = new SqlCommand(checkSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@dbName", DatabaseName);
                        int exists = (int)cmd.ExecuteScalar();

                        if (exists > 0)
                        {
                            string detachSql = $@"
                                USE master;
                                ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                EXEC sp_detach_db '{DatabaseName}';";
                            try
                            {
                                using (var detachCmd = new SqlCommand(detachSql, connection))
                                {
                                    detachCmd.CommandTimeout = 30;
                                    detachCmd.ExecuteNonQuery();
                                }
                            }
                            catch
                            {
                                // База могла быть уже отсоединена
                            }
                        }
                    }

                    Debug.WriteLine($"Attach database: {mdfPath}");

                    string attachSql = $@"
                        CREATE DATABASE [{DatabaseName}] ON 
                        (FILENAME = N'{mdfPath}')";

                    if (File.Exists(ldfPath))
                    {
                        attachSql += $@",
                        (FILENAME = N'{ldfPath}')";
                    }

                    attachSql += " FOR ATTACH;";

                    using (var attachCmd = new SqlCommand(attachSql, connection))
                    {
                        attachCmd.CommandTimeout = 60;
                        attachCmd.ExecuteNonQuery();
                    }
                }

                ReseedIdentityColumnsIfNeeded();

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
                DetachDatabase();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Detach on exit: {ex.Message}");
            }
        }

        private void DetachDatabase()
        {
            string masterConnString = $"Data Source={LocalDbInstance};Integrated Security=True;";

            using (var connection = new SqlConnection(masterConnString))
            {
                connection.Open();

                string detachSql = $@"
                    USE master;
                    
                    DECLARE @killSql NVARCHAR(MAX) = '';
                    SELECT @killSql = @killSql + 'KILL ' + CAST(session_id AS NVARCHAR) + ';'
                    FROM sys.dm_exec_sessions
                    WHERE database_id = DB_ID('{DatabaseName}');
                    
                    IF @killSql != ''
                    BEGIN
                        EXEC sp_executesql @killSql;
                        WAITFOR DELAY '00:00:01';
                    END
                    
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

        private void StartLocalDb()
        {
            try
            {
                var startInfo = new ProcessStartInfo("sqllocaldb", "start MSSQLLocalDB")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process?.WaitForExit(10000);
                }

                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"StartLocalDb: {ex.Message}");
            }
        }

        private void ReseedIdentityColumnsIfNeeded()
        {
            try
            {
                string connString = $"Data Source={LocalDbInstance};Initial Catalog={DatabaseName};Integrated Security=True;";
                using (var connection = new SqlConnection(connString))
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(@"
DECLARE @table SYSNAME, @maxId INT, @sql NVARCHAR(MAX);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT t.name
    FROM sys.tables t
    INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.name = 'Id' AND c.is_identity = 1
    WHERE t.schema_id = SCHEMA_ID('dbo');
OPEN cur;
FETCH NEXT FROM cur INTO @table;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'SELECT @m = ISNULL(MAX(Id), 0) FROM ' + QUOTENAME(@table);
    EXEC sp_executesql @sql, N'@m INT OUTPUT', @m = @maxId OUTPUT;
    IF @maxId > CAST(IDENT_CURRENT(@table) AS INT)
    BEGIN
        SET @sql = N'DBCC CHECKIDENT (''' + @table + ''', RESEED, ' + CAST(@maxId AS NVARCHAR(20)) + ')';
        EXEC(@sql);
        PRINT 'Reseeded ' + @table + ' to ' + CAST(@maxId AS NVARCHAR(20));
    END
    FETCH NEXT FROM cur INTO @table;
END
CLOSE cur;
DEALLOCATE cur;", connection))
                    {
                        cmd.CommandTimeout = 60;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ReseedIdentityColumnsIfNeeded: {ex.Message}");
            }
        }
    }
}