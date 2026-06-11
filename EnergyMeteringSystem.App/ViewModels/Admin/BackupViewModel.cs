using System;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using Microsoft.Win32;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class BackupViewModel : ViewModelBase
    {
        private string _backupPath;
        private string _restorePath;
        private string _statusMessage;

        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty(ref _backupPath, value);
        }

        public string RestorePath
        {
            get => _restorePath;
            set => SetProperty(ref _restorePath, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Команды (теперь асинхронные)
        public AsyncRelayCommand BrowseBackupCommand { get; }
        public AsyncRelayCommand BrowseRestoreCommand { get; }
        public AsyncRelayCommand CreateBackupCommand { get; }
        public AsyncRelayCommand RestoreCommand { get; }

        public BackupViewModel()
        {
            // Путь по умолчанию для бэкапов
            string defaultBackupFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EnergyMeteringBackups");

            if (!Directory.Exists(defaultBackupFolder))
                Directory.CreateDirectory(defaultBackupFolder);

            string dbName = GetDatabaseName();
            BackupPath = Path.Combine(defaultBackupFolder, $"{dbName}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
            RestorePath = BackupPath;

            BrowseBackupCommand = new AsyncRelayCommand(async () => await BrowseBackupAsync());
            BrowseRestoreCommand = new AsyncRelayCommand(async () => await BrowseRestoreAsync());
            CreateBackupCommand = new AsyncRelayCommand(async () => await CreateBackupAsync(), () => !IsLoading);
            RestoreCommand = new AsyncRelayCommand(async () => await RestoreAsync(), () => !IsLoading && File.Exists(RestorePath));

            StatusMessage = "Готов к работе";
        }

        // Получение строки подключения к БД
        private string GetSqlConnectionString()
        {
            string efConnectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["EnergyMeteringSystemEntities"].ConnectionString;

            var builder = new EntityConnectionStringBuilder(efConnectionString);
            return builder.ProviderConnectionString;
        }

        // Имя базы данных из строки подключения
        private string GetDatabaseName()
        {
            string connString = GetSqlConnectionString();
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder(connString);
            return builder.InitialCatalog;
        }

        private async Task BrowseBackupAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var dialog = new SaveFileDialog
                    {
                        Title = "Сохранить резервную копию как",
                        FileName = Path.GetFileName(BackupPath),
                        DefaultExt = ".bak",
                        Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                        InitialDirectory = Path.GetDirectoryName(BackupPath),
                        OverwritePrompt = true
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        BackupPath = dialog.FileName;
                        StatusMessage = $"Путь сохранения: {BackupPath}";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка: {ex.Message}";
                    MessageBox.Show($"Ошибка при выборе пути: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private async Task BrowseRestoreAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = "Выберите файл резервной копии",
                        DefaultExt = ".bak",
                        Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        CheckFileExists = true,
                        Multiselect = false
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        RestorePath = dialog.FileName;
                        StatusMessage = $"Выбран файл для восстановления: {Path.GetFileName(RestorePath)}";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Ошибка: {ex.Message}";
                    MessageBox.Show($"Ошибка при выборе файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private async Task CreateBackupAsync()
        {
            await ExecuteAsync(async () =>
            {
                await Task.Run(() =>
                {
                    string databaseName = GetDatabaseName();
                    string sqlConnectionString = GetSqlConnectionString();

                    string backupCommand = $@"
                        BACKUP DATABASE [{databaseName}] 
                        TO DISK = '{BackupPath}' 
                        WITH FORMAT, 
                             MEDIANAME = 'SQLServerBackup', 
                             NAME = 'Full Backup of {databaseName}';";

                    using (var connection = new System.Data.SqlClient.SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        using (var command = new System.Data.SqlClient.SqlCommand(backupCommand, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    // Проверяем размер файла
                    var fileInfo = new FileInfo(BackupPath);
                    if (fileInfo.Exists && fileInfo.Length > 0)
                    {
                        StatusMessage = $"Резервная копия создана: {Path.GetFileName(BackupPath)} ({fileInfo.Length / 1024 / 1024} MB)";

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Резервное копирование выполнено успешно!\n\n" +
                                $"Файл: {BackupPath}\n" +
                                $"Размер: {fileInfo.Length / 1024 / 1024} MB",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    }
                    else
                    {
                        StatusMessage = "Ошибка: файл бэкапа не создан";
                        throw new Exception("Резервная копия не была создана");
                    }
                });
            }, "Ошибка при создании резервной копии");
        }

        private async Task RestoreAsync()
        {
            if (!File.Exists(RestorePath))
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show("Файл резервной копии не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return;
            }

            var confirmResult = await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                return MessageBox.Show(
                    "ВНИМАНИЕ! Восстановление из резервной копии ЗАМЕНИТ все текущие данные!\n\n" +
                    "Все несохранённые изменения будут потеряны.\n" +
                    "Рекомендуется перед восстановлением создать резервную копию текущей базы.\n\n" +
                    "Продолжить?",
                    "Подтверждение восстановления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
            });

            if (confirmResult != MessageBoxResult.Yes)
                return;

            await ExecuteAsync(async () =>
            {
                await Task.Run(() =>
                {
                    string databaseName = GetDatabaseName();
                    string sqlConnectionString = GetSqlConnectionString();

                    // Получаем пути к файлам базы данных
                    string mdfPath = "", ldfPath = "";
                    GetDatabaseFilesPaths(sqlConnectionString, databaseName, ref mdfPath, ref ldfPath);

                    string restoreCommand = $@"
                        USE master;
                        
                        ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        
                        RESTORE DATABASE [{databaseName}] 
                        FROM DISK = '{RestorePath}' 
                        WITH REPLACE,
                             MOVE '{databaseName}' TO '{mdfPath}',
                             MOVE '{databaseName}_log' TO '{ldfPath}';
                        
                        ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                    using (var connection = new System.Data.SqlClient.SqlConnection(sqlConnectionString))
                    {
                        connection.Open();
                        using (var command = new System.Data.SqlClient.SqlCommand(restoreCommand, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    StatusMessage = $"Восстановление завершено из файла: {Path.GetFileName(RestorePath)}";

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Восстановление выполнено успешно!\n\n" +
                            $"Файл: {RestorePath}\n\n" +
                            $"Рекомендуется перезапустить приложение для обновления данных.",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                });
            }, "Ошибка при восстановлении");
        }

        private void GetDatabaseFilesPaths(string connectionString, string databaseName, ref string mdfPath, ref string ldfPath)
        {
            string query = $@"
                SELECT physical_name 
                FROM sys.master_files 
                WHERE database_id = DB_ID('{databaseName}')";

            using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string path = reader["physical_name"].ToString();
                        if (path.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
                            mdfPath = path;
                        else if (path.EndsWith(".ldf", StringComparison.OrdinalIgnoreCase))
                            ldfPath = path;
                    }
                }
            }

            if (string.IsNullOrEmpty(mdfPath))
                mdfPath = @"C:\Program Files\Microsoft SQL Server\MSSQL\DATA\";
            if (string.IsNullOrEmpty(ldfPath))
                ldfPath = mdfPath.Replace(".mdf", "_log.ldf");
        }
    }
}