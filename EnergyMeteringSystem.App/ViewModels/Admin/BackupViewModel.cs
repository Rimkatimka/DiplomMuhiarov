using System;
using System.Data.Entity.Core.EntityClient;
using System.IO;
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
        private bool _isBusy;

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

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public RelayCommand BrowseBackupCommand { get; }
        public RelayCommand BrowseRestoreCommand { get; }
        public RelayCommand CreateBackupCommand { get; }
        public RelayCommand RestoreCommand { get; }

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

            BrowseBackupCommand = new RelayCommand(_ => BrowseBackup());
            BrowseRestoreCommand = new RelayCommand(_ => BrowseRestore());
            CreateBackupCommand = new RelayCommand(_ => CreateBackup(), _ => !IsBusy);
            RestoreCommand = new RelayCommand(_ => Restore(), _ => !IsBusy && File.Exists(RestorePath));

            StatusMessage = "Готов к работе";
        }

        private void BrowseBackup()
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
                MessageBox.Show($"Ошибка при выборе пути: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseRestore()
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
                MessageBox.Show($"Ошибка при выборе файла: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateBackup()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Создание резервной копии...";

                string databaseName = GetDatabaseName();
                string sqlConnectionString = GetSqlConnectionString();

                // ✅ УБРАЛИ COMPRESSION (не поддерживается в Express Edition)
                string backupCommand = $@"
            BACKUP DATABASE [{databaseName}] 
            TO DISK = '{BackupPath}' 
            WITH FORMAT, 
                 MEDIANAME = 'SQLServerBackup', 
                 NAME = 'Full Backup of {databaseName}';";

                using (var connection = new System.Data.SqlClient.SqlConnection(sqlConnectionString))
                {
                    var command = new System.Data.SqlClient.SqlCommand(backupCommand, connection);
                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    // Проверяем размер файла
                    var fileInfo = new FileInfo(BackupPath);
                    if (fileInfo.Exists && fileInfo.Length > 0)
                    {
                        StatusMessage = $"Резервная копия создана: {Path.GetFileName(BackupPath)} ({fileInfo.Length / 1024 / 1024} MB)";
                        MessageBox.Show($"Резервное копирование выполнено успешно!\n\n" +
                            $"Файл: {BackupPath}\n" +
                            $"Размер: {fileInfo.Length / 1024 / 1024} MB",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = "Ошибка: файл бэкапа не создан";
                        MessageBox.Show("Резервная копия не была создана. Проверьте права доступа.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при создании бэкапа:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Restore()
        {
            if (!File.Exists(RestorePath))
            {
                MessageBox.Show("Файл резервной копии не найден!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "ВНИМАНИЕ! Восстановление из резервной копии ЗАМЕНИТ все текущие данные!\n\n" +
                "Все несохранённые изменения будут потеряны.\n" +
                "Рекомендуется перед восстановлением создать резервную копию текущей базы.\n\n" +
                "Продолжить?",
                "Подтверждение восстановления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsBusy = true;
                StatusMessage = "Подготовка к восстановлению...";

                string databaseName = GetDatabaseName();
                string sqlConnectionString = GetSqlConnectionString();

                // Получаем пути к файлам базы данных
                string mdfPath = "", ldfPath = "";
                GetDatabaseFilesPaths(sqlConnectionString, databaseName, ref mdfPath, ref ldfPath);

                string restoreCommand = $@"
                    USE master;
                    
                    -- Закрываем все соединения с базой
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    
                    -- Восстанавливаем базу
                    RESTORE DATABASE [{databaseName}] 
                    FROM DISK = '{RestorePath}' 
                    WITH REPLACE,
                         MOVE '{databaseName}' TO '{mdfPath}',
                         MOVE '{databaseName}_log' TO '{ldfPath}';
                    
                    -- Возвращаем базу в многопользовательский режим
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using (var connection = new System.Data.SqlClient.SqlConnection(sqlConnectionString))
                {
                    var command = new System.Data.SqlClient.SqlCommand(restoreCommand, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }

                StatusMessage = $"Восстановление завершено из файла: {Path.GetFileName(RestorePath)}";
                MessageBox.Show($"Восстановление выполнено успешно!\n\n" +
                    $"Файл: {RestorePath}\n\n" +
                    $"Рекомендуется перезапустить приложение для обновления данных.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при восстановлении:\n{ex.Message}\n\n" +
                    "Возможно, база данных используется другим процессом.\n" +
                    "Закройте все подключения к базе и повторите попытку.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void GetDatabaseFilesPaths(string connectionString, string databaseName, ref string mdfPath, ref string ldfPath)
        {
            string query = $@"
                SELECT 
                    physical_name 
                FROM sys.master_files 
                WHERE database_id = DB_ID('{databaseName}')";

            using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                connection.Open();
                var command = new System.Data.SqlClient.SqlCommand(query, connection);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string path = reader["physical_name"].ToString();
                    if (path.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase))
                        mdfPath = path;
                    else if (path.EndsWith(".ldf", StringComparison.OrdinalIgnoreCase))
                        ldfPath = path;
                }
                reader.Close();
            }

            if (string.IsNullOrEmpty(mdfPath))
                mdfPath = @"C:\Program Files\Microsoft SQL Server\MSSQL\DATA\";
            if (string.IsNullOrEmpty(ldfPath))
                ldfPath = mdfPath.Replace(".mdf", "_log.ldf");
        }
    }
}