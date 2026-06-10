using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingVerificationViewModel : INotifyPropertyChanged
    {
        private readonly MeterReadingRepository _repository;
        private ObservableCollection<MeterReadingVerificationDto> _readings;
        private MeterReadingVerificationDto _selectedReading;
        private bool _isBatchMode;
        private bool _isRejectionMode;

        public ObservableCollection<MeterReadingVerificationDto> Readings
        {
            get => _readings;
            set
            {
                _readings = value;
                OnPropertyChanged();
            }
        }

        public MeterReadingVerificationDto SelectedReading
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

                if (!value && Readings != null)
                {
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

        public MeterReadingVerificationViewModel()
        {
            _repository = new MeterReadingRepository();

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
                var readings = _repository.GetForVerification();
                Readings = new ObservableCollection<MeterReadingVerificationDto>(readings);
                System.Diagnostics.Debug.WriteLine($"Загружено {readings.Count} показаний");
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
                var readingsToVerify = IsBatchMode
                    ? Readings.Where(r => r.IsSelected).ToList()
                    : new List<MeterReadingVerificationDto> { SelectedReading };

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
                    _repository.UpdateStatus(reading.Id, 2);
                    successCount++;

                    if (IsBatchMode)
                        Readings.Remove(reading);
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
                    : new List<MeterReadingVerificationDto> { SelectedReading };

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
                    _repository.UpdateStatus(reading.Id, 3, SelectedReason.Id, RejectionComment);
                    successCount++;

                    if (IsBatchMode)
                        Readings.Remove(reading);
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
                MessageBox.Show($"Ошибка при отклонении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectAll()
        {
            if (Readings == null || !IsBatchMode) return;

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

    public class RejectionReason
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}