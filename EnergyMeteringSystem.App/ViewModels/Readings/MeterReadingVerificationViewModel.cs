using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingVerificationViewModel : ViewModelBase
    {
        private readonly MeterReadingRepository _repository;
        private ObservableCollection<MeterReadingVerificationDto> _readings;
        private MeterReadingVerificationDto _selectedReading;
        private bool _isBatchMode;
        private bool _isRejectionMode;
        private RejectionReason _selectedReason;
        private string _rejectionComment;

        public ObservableCollection<MeterReadingVerificationDto> Readings
        {
            get => _readings;
            set => SetProperty(ref _readings, value);
        }

        public MeterReadingVerificationDto SelectedReading
        {
            get => _selectedReading;
            set
            {
                SetProperty(ref _selectedReading, value);
                (VerifyCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsBatchMode
        {
            get => _isBatchMode;
            set
            {
                SetProperty(ref _isBatchMode, value);

                if (!value && Readings != null)
                {
                    foreach (var reading in Readings)
                    {
                        reading.IsSelected = false;
                    }
                }
                (VerifyCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsRejectionMode
        {
            get => _isRejectionMode;
            set => SetProperty(ref _isRejectionMode, value);
        }

        public ObservableCollection<RejectionReason> RejectionReasons { get; set; }

        public RejectionReason SelectedReason
        {
            get => _selectedReason;
            set
            {
                SetProperty(ref _selectedReason, value);
                (ConfirmRejectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string RejectionComment
        {
            get => _rejectionComment;
            set => SetProperty(ref _rejectionComment, value);
        }

        public AsyncRelayCommand RefreshCommand { get; }
        public AsyncRelayCommand SelectAllCommand { get; }
        public AsyncRelayCommand VerifyCommand { get; }
        public AsyncRelayCommand RejectCommand { get; }
        public RelayCommand CancelRejectCommand { get; }
        public AsyncRelayCommand ConfirmRejectCommand { get; }

        public MeterReadingVerificationViewModel()
        {
            _repository = new MeterReadingRepository();

            RefreshCommand = new AsyncRelayCommand(async () => await LoadReadingsAsync());
            SelectAllCommand = new AsyncRelayCommand(() => { SelectAll(); return Task.CompletedTask; }, () => IsBatchMode);
            VerifyCommand = new AsyncRelayCommand(async () => await VerifyAsync(), () => CanVerify());
            RejectCommand = new AsyncRelayCommand(() => { ShowRejectionMode(); return Task.CompletedTask; }, () => CanReject());
            CancelRejectCommand = new RelayCommand(_ => CancelRejection());
            ConfirmRejectCommand = new AsyncRelayCommand(async () => await ConfirmRejectionAsync(), () => CanConfirmRejection());

            LoadRejectionReasons();
            _ = LoadReadingsAsync();
        }

        private async Task LoadReadingsAsync()
        {
            await ExecuteAsync(async () =>
            {
                var readings = await _repository.GetForVerificationAsync();
                Readings = new ObservableCollection<MeterReadingVerificationDto>(readings);
                System.Diagnostics.Debug.WriteLine($"Загружено {readings.Count} показаний");
            }, "Ошибка загрузки показаний");
        }

        private bool CanVerify()
        {
            if (IsBatchMode)
                return Readings != null && Readings.Any(r => r.IsSelected);
            else
                return SelectedReading != null && SelectedReading.Id > 0;
        }

        private async Task VerifyAsync()
        {
            await ExecuteAsync(async () =>
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
                    await _repository.UpdateStatusAsync(reading.Id, 2);
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

                (VerifyCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }, "Ошибка при верификации");
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

        private async Task ConfirmRejectionAsync()
        {
            await ExecuteAsync(async () =>
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
                    await _repository.UpdateStatusAsync(reading.Id, 3, SelectedReason.Id, RejectionComment);
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

                (VerifyCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (RejectCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }, "Ошибка при отклонении");
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
    }

    public class RejectionReason
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}