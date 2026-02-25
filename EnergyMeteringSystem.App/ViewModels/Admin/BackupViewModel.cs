using System;
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
            // Путь по умолчанию
            BackupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EnergyMeteringBackup.bak");

            BrowseCommand = new RelayCommand(_ => Browse());
            CreateBackupCommand = new RelayCommand(_ => CreateBackup(), _ => !IsBusy);
            RestoreCommand = new RelayCommand(_ => Restore(), _ => !IsBusy && File.Exists(BackupPath));

            StatusMessage = "Готов к работе";
        }

        private void Browse()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "EnergyMeteringBackup",
                DefaultExt = ".bak",
                Filter = "Backup files (.bak)|*.bak"
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

                // Здесь будет код бэкапа через SQL
                System.Threading.Thread.Sleep(2000); // Имитация работы

                StatusMessage = $"Резервная копия создана: {BackupPath}";
                MessageBox.Show("Резервное копирование выполнено успешно", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при создании бэкапа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Restore()
        {
            var result = MessageBox.Show(
                "Восстановление из резервной копии заменит все текущие данные. Продолжить?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsBusy = true;
                StatusMessage = "Восстановление из резервной копии...";

                // Здесь будет код восстановления
                System.Threading.Thread.Sleep(3000); // Имитация работы

                StatusMessage = "Восстановление завершено";
                MessageBox.Show("Восстановление выполнено успешно", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка при восстановлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}