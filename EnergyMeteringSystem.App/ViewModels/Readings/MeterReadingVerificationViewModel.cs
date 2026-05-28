using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Data.SqlClient;
using System.Collections.Generic;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.Services;
using EnergyMeteringSystem.App.DTO;
using EnergyMeteringSystem.Data.Database;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingVerificationViewModel : INotifyPropertyChanged
    {
        private readonly IDatabaseService _databaseService;
        private ObservableCollection<ReadingVerificationDto> _readings;
        private ReadingVerificationDto _selectedReading;
        private bool _isBatchMode;
        private bool _isRejectionMode;

        public ObservableCollection<ReadingVerificationDto> Readings
        {
            get => _readings;
            set
            {
                _readings = value;
                OnPropertyChanged();
            }
        }

        public ReadingVerificationDto SelectedReading
        {
            get => _selectedReading;
            set
            {
                _selectedReading = value;
                OnPropertyChanged();
                (VerifyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsBatchMode
        {
            get => _isBatchMode;
            set
            {
                _isBatchMode = value;
                OnPropertyChanged();

                if (!value)
                {
                    // При выходе из пакетного режима снимаем все галочки
                    foreach (var reading in Readings)
                    {
                        reading.IsSelected = false;
                    }
                }

                (VerifyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsRejectionMode
        {
            get => _isRejectionMode;
            set
            {
                _isRejectionMode = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand VerifyCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand CancelRejectCommand { get; }
        public ICommand ConfirmRejectCommand { get; }

        public ObservableCollection<RejectionReason> RejectionReasons { get; set; }

        private RejectionReason _selectedReason;
        public RejectionReason SelectedReason
        {
            get => _selectedReason;
            set
            {
                _selectedReason = value;
                OnPropertyChanged();
                (ConfirmRejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _rejectionComment;
        public string RejectionComment
        {
            get => _rejectionComment;
            set
            {
                _rejectionComment = value;
                OnPropertyChanged();
            }
        }

        public MeterReadingVerificationViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            RefreshCommand = new RelayCommand(_ => LoadReadings());
            SelectAllCommand = new RelayCommand(_ => SelectAll(), _ => IsBatchMode);
            VerifyCommand = new RelayCommand(_ => Verify(), _ => CanVerify());
            RejectCommand = new RelayCommand(_ => ShowRejectionMode(), _ => CanReject());
            CancelRejectCommand = new RelayCommand(_ => CancelRejection());
            ConfirmRejectCommand = new RelayCommand(_ => ConfirmRejection(), _ => CanConfirmRejection());

            LoadRejectionReasons();
            LoadReadings();
        }

        private void LoadReadings()
        {
            try
            {
                string query = @"
                SELECT 
                    r.Id,
                    s.Name as Address,
                    m.SerialNumber,
                    r.ReadingDate,
                    r.Value,
                    u.FullName as EnteredBy,
                    r.EnteredAt
                FROM MeterReading r
                INNER JOIN Meter m ON r.MeterId = m.Id
                INNER JOIN ConsumptionObject o ON m.ConsumptionObjectId = o.Id
                INNER JOIN Street s ON o.StreetId = s.Id
                LEFT JOIN [User] u ON r.EnteredByUserId = u.Id
                WHERE r.ReadingStatusId = 1
                ORDER BY r.ReadingDate DESC";

                var readings = _databaseService.ExecuteQuery<ReadingVerificationDto>(query);

                foreach (var reading in readings)
                {
                    reading.IsSelected = false;  // ← ЯВНО УСТАНАВЛИВАЕМ ЗНАЧЕНИЕ
                }

                Readings = new ObservableCollection<ReadingVerificationDto>(readings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadReadings error: {ex.Message}");
                MessageBox.Show($"Ошибка загрузки показаний: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanVerify()
        {
            if (IsBatchMode)
                return Readings != null && Readings.Any(r => r.IsSelected);
            else
                return SelectedReading != null && SelectedReading.Id > 0;
        }

        private void Verify()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== VERIFY CALLED ===");

                var readingsToVerify = IsBatchMode
                    ? Readings.Where(r => r.IsSelected).ToList()
                    : new List<ReadingVerificationDto> { SelectedReading };

                if (!readingsToVerify.Any())
                {
                    MessageBox.Show("Нет выбранных показаний для верификации", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите верифицировать {readingsToVerify.Count} показание(й)?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                int successCount = 0;

                foreach (var reading in readingsToVerify)
                {
                    System.Diagnostics.Debug.WriteLine($"Verify: показание ID={reading.Id}");

                    string updateQuery = @"
                        UPDATE MeterReading 
                        SET ReadingStatusId = 2 
                        WHERE Id = @Id AND ReadingStatusId = 1";

                    int rowsAffected = _databaseService.ExecuteNonQuery(updateQuery,
                        new SqlParameter("@Id", reading.Id));

                    if (rowsAffected > 0)
                    {
                        successCount++;
                        if (IsBatchMode)
                            Readings.Remove(reading);
                    }
                }

                if (!IsBatchMode && SelectedReading != null && successCount > 0)
                {
                    Readings.Remove(SelectedReading);
                    SelectedReading = null;
                }

                MessageBox.Show($"Верифицировано: {successCount}", "Результат",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                (VerifyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Verify error: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanReject()
        {
            if (IsBatchMode)
                return Readings != null && Readings.Any(r => r.IsSelected);
            else
                return SelectedReading != null && SelectedReading.Id > 0;
        }

        private void ShowRejectionMode()
        {
            if (!CanReject())
            {
                MessageBox.Show("Выберите показание для отклонения", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsRejectionMode = true;
        }

        private void CancelRejection()
        {
            IsRejectionMode = false;
            SelectedReason = null;
            RejectionComment = string.Empty;
        }

        private bool CanConfirmRejection()
        {
            return SelectedReason != null;
        }

        private void ConfirmRejection()
        {
            try
            {
                var readingsToReject = IsBatchMode
                    ? Readings.Where(r => r.IsSelected).ToList()
                    : new List<ReadingVerificationDto> { SelectedReading };

                if (!readingsToReject.Any())
                {
                    MessageBox.Show("Нет выбранных показаний для отклонения", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите отклонить {readingsToReject.Count} показание(й)?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                int successCount = 0;

                foreach (var reading in readingsToReject)
                {
                    System.Diagnostics.Debug.WriteLine($"Reject: отклонение показания ID={reading.Id}");

                    string updateQuery = @"
                        UPDATE MeterReading 
                        SET ReadingStatusId = 3,
                            RejectionReasonId = @ReasonId,
                            Comment = @Comment
                        WHERE Id = @Id AND ReadingStatusId = 1";

                    int rowsAffected = _databaseService.ExecuteNonQuery(updateQuery,
                        new SqlParameter("@Id", reading.Id),
                        new SqlParameter("@ReasonId", SelectedReason?.Id ?? 1),
                        new SqlParameter("@Comment", RejectionComment ?? ""));

                    if (rowsAffected > 0)
                    {
                        successCount++;
                        System.Diagnostics.Debug.WriteLine($"Reject: показание ID={reading.Id} успешно отклонено");

                        if (IsBatchMode)
                            Readings.Remove(reading);
                    }
                }

                if (!IsBatchMode && SelectedReading != null && successCount > 0)
                {
                    Readings.Remove(SelectedReading);
                    SelectedReading = null;
                }

                CancelRejection();

                MessageBox.Show($"Отклонено показаний: {successCount}", "Результат",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                (VerifyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reject error: {ex.Message}");
                MessageBox.Show($"Ошибка при отклонении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectAll()
        {
            if (Readings == null || !IsBatchMode) return;

            // Проверяем, все ли уже выбраны
            bool allSelected = Readings.All(r => r.IsSelected);
            bool newValue = !allSelected;

            foreach (var reading in Readings)
            {
                reading.IsSelected = newValue;
            }
        }

        private void LoadRejectionReasons()
        {
            RejectionReasons = new ObservableCollection<RejectionReason>
            {
                new RejectionReason { Id = 1, Name = "Неверные показания" },
                new RejectionReason { Id = 2, Name = "Дублирующая запись" },
                new RejectionReason { Id = 3, Name = "Нет доступа к счётчику" },
                new RejectionReason { Id = 4, Name = "Счётчик неисправен" },
                new RejectionReason { Id = 5, Name = "Показания не соответствуют норме" },
                new RejectionReason { Id = 6, Name = "Другое" }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}