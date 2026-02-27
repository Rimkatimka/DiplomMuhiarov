using System;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class BackupViewModel : ViewModelBase
    {
        private string _backupPath;
        private string _statusMessage;
        private bool _isBusy;
        private string GetSqlConnectionString()
        {
            // Получаем EF-строку
            string efConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["EnergyMeteringSystemEntities"].ConnectionString;

            // Извлекаем обычную SQL-строку из метаданных
            EntityConnectionStringBuilder builder = new(efConnectionString);
            return builder.ProviderConnectionString;
        }

        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty(ref _backupPath, value);
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

        public RelayCommand BrowseCommand { get; }
        public RelayCommand CreateBackupCommand { get; }
        public RelayCommand RestoreCommand { get; }

        public BackupViewModel()
        {
            // Папка Backups в корне проекта
            string solutionPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\"));
            string backupFolder = @"C:\Users\Рим Мухияров\Desktop\DiplomMuhiarov\EnergyMeteringSystem.App\Backups";
            if (!Directory.Exists(backupFolder))
            {
                _ = Directory.CreateDirectory(backupFolder);
            }

            BackupPath = Path.Combine(backupFolder, "EnergyMeteringBackup.bak");

            BrowseCommand = new RelayCommand(_ => Browse());
            CreateBackupCommand = new RelayCommand(_ => CreateBackup(), _ => !IsBusy);
            RestoreCommand = new RelayCommand(_ => Restore(), _ => !IsBusy && File.Exists(BackupPath));

            StatusMessage = "Готов к работе";
        }

        private void Browse()
        {
            Microsoft.Win32.SaveFileDialog dialog = new()
            {
                FileName = "EnergyMeteringBackup",
                DefaultExt = ".bak",
                Filter = "Backup files (*.bak)|*.bak",
                InitialDirectory = Path.GetDirectoryName(BackupPath)
            };

            if (dialog.ShowDialog() == true)
            {
                BackupPath = dialog.FileName;
            }
        }

        private void CreateBackup()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Создание резервной копии...";

                string databaseName = "EnergyMeteringSystem";

                // Получаем обычную SQL-строку подключения
                string sqlConnectionString = GetSqlConnectionString();

                string backupCommand = $@"
            BACKUP DATABASE [{databaseName}] 
            TO DISK = '{BackupPath}' 
            WITH FORMAT, 
                 MEDIANAME = 'SQLServerBackup', 
                 NAME = 'Full Backup of {databaseName}';";

                using (System.Data.SqlClient.SqlConnection connection = new(sqlConnectionString))
                {
                    System.Data.SqlClient.SqlCommand command = new(backupCommand, connection);
                    connection.Open();
                    _ = command.ExecuteNonQuery();
                }

                StatusMessage = $"Резервная копия создана: {BackupPath}";
                _ = MessageBox.Show("Резервное копирование выполнено успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                _ = MessageBox.Show($"Ошибка при создании бэкапа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private void Restore()
        {
            MessageBoxResult result = MessageBox.Show(
                "Восстановление из резервной копии заменит все текущие данные!\n\n" +
                "Все несохранённые изменения будут потеряны.\n\n" +
                "Продолжить?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Восстановление из резервной копии...";

                string databaseName = "EnergyMeteringSystem";

                // Переводим БД в SINGLE_USER режим и восстанавливаем
                string restoreCommand = $@"
                    USE master;
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{databaseName}] 
                    FROM DISK = '{BackupPath}' 
                    WITH REPLACE;
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using (System.Data.SqlClient.SqlConnection connection = new(
                    System.Configuration.ConfigurationManager.ConnectionStrings["EnergyMeteringSystemEntities"].ConnectionString))
                {
                    System.Data.SqlClient.SqlCommand command = new(restoreCommand, connection);
                    connection.Open();
                    _ = command.ExecuteNonQuery();
                }

                StatusMessage = "Восстановление завершено";
                _ = MessageBox.Show("Восстановление выполнено успешно!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                _ = MessageBox.Show($"Ошибка при восстановлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}