using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Readings
{
    public class MeterReadingVerificationViewModel : ViewModelBase
    {
        private readonly MeterReadingRepository _readingRepository;
        private readonly RejectionReasonRepository _reasonRepository;

        private MeterReadingVerificationDto _selectedReading;
        private RejectionReasonDto _selectedReason;
        private string _rejectionComment;
        private bool _isBatchMode;
        private bool _isRejectionMode;

        public ObservableCollection<MeterReadingVerificationDto> Readings { get; set; }
        public ObservableCollection<RejectionReasonDto> RejectionReasons { get; set; }
        public MeterReadingVerificationDto SelectedReading
        {
            get => _selectedReading;
            set
            {
                _ = SetProperty(ref _selectedReading, value);
                VerifyCommand.RaiseCanExecuteChanged();
                RejectCommand.RaiseCanExecuteChanged();
            }
        }

        public RejectionReasonDto SelectedReason
        {
            get => _selectedReason;
            set
            {
                _ = SetProperty(ref _selectedReason, value);
                ConfirmRejectCommand.RaiseCanExecuteChanged();
            }
        }

        public string RejectionComment
        {
            get => _rejectionComment;
            set
            {
                _ = SetProperty(ref _rejectionComment, value);
                ConfirmRejectCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsBatchMode
        {
            get => _isBatchMode;
            set => SetProperty(ref _isBatchMode, value);
        }

        public bool IsRejectionMode
        {
            get => _isRejectionMode;
            set => SetProperty(ref _isRejectionMode, value);
        }

        public bool HasSelectedAny => Readings?.Any(r => r.IsSelected) == true;

        public RelayCommand RefreshCommand { get; }
        public RelayCommand VerifyCommand { get; }
        public RelayCommand RejectCommand { get; }
        public RelayCommand ConfirmRejectCommand { get; }
        public RelayCommand CancelRejectCommand { get; }
        public RelayCommand ToggleBatchModeCommand { get; }
        public RelayCommand SelectAllCommand { get; }

        public MeterReadingVerificationViewModel()
        {
            _readingRepository = new MeterReadingRepository();
            _reasonRepository = new RejectionReasonRepository();

            Readings = [];
            RejectionReasons = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            VerifyCommand = new RelayCommand(_ => Verify(), _ => CanVerify());
            RejectCommand = new RelayCommand(_ => StartReject(), _ => CanReject());
            ConfirmRejectCommand = new RelayCommand(_ => ConfirmReject(), _ => CanConfirmReject());
            CancelRejectCommand = new RelayCommand(_ => CancelReject());
            ToggleBatchModeCommand = new RelayCommand(_ => ToggleBatchMode());
            SelectAllCommand = new RelayCommand(_ => SelectAll());

            LoadData();
            LoadRejectionReasons();
        }

        private void LoadData()
        {
            Readings.Clear();
            List<MeterReadingVerificationDto> list = _readingRepository.GetForVerification();
            foreach (MeterReadingVerificationDto item in list)
            {
                Readings.Add(item);
            }
        }

        private void LoadRejectionReasons()
        {
            RejectionReasons.Clear();
            List<DirectoryDto> list = _reasonRepository.GetAll(); // это List<DirectoryDto>

            foreach (DirectoryDto item in list)
            {
                RejectionReasons.Add(new RejectionReasonDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    RequiresComment = item.Description?.Contains("Требует") ?? false
                });
            }
        }

        private bool CanVerify()
        {
            return IsBatchMode ? HasSelectedAny : SelectedReading != null;
        }

        private void Verify()
        {
            if (IsBatchMode)
            {
                List<MeterReadingVerificationDto> selected = Readings.Where(r => r.IsSelected).ToList();
                foreach (MeterReadingVerificationDto item in selected)
                {
                    _readingRepository.UpdateStatus(item.Id, 2); // "Подтверждено"
                }
            }
            else
            {
                _readingRepository.UpdateStatus(SelectedReading.Id, 2);
            }

            LoadData();
            IsRejectionMode = false;
        }

        private bool CanReject()
        {
            return IsBatchMode ? HasSelectedAny : SelectedReading != null;
        }

        private void StartReject()
        {
            IsRejectionMode = true;
            SelectedReason = null;
            RejectionComment = string.Empty;
        }

        private bool CanConfirmReject()
        {
            return SelectedReason != null && (!SelectedReason.RequiresComment || !string.IsNullOrWhiteSpace(RejectionComment));
        }

        private void ConfirmReject()
        {
            int newStatusId = 3; // "Отклонено"

            if (IsBatchMode)
            {
                List<MeterReadingVerificationDto> selected = Readings.Where(r => r.IsSelected).ToList();
                foreach (MeterReadingVerificationDto item in selected)
                {
                    _readingRepository.UpdateStatus(item.Id, newStatusId, SelectedReason?.Id, RejectionComment);
                }
            }
            else
            {
                _readingRepository.UpdateStatus(SelectedReading.Id, newStatusId, SelectedReason?.Id, RejectionComment);
            }

            LoadData();
            IsRejectionMode = false;
        }

        private void CancelReject()
        {
            IsRejectionMode = false;
        }

        private void ToggleBatchMode()
        {
            IsBatchMode = !IsBatchMode;
            if (!IsBatchMode)
            {
                foreach (MeterReadingVerificationDto item in Readings)
                {
                    item.IsSelected = false;
                }
            }
        }

        private void SelectAll()
        {
            bool allSelected = Readings.All(r => r.IsSelected);
            foreach (MeterReadingVerificationDto item in Readings)
            {
                item.IsSelected = !allSelected;
            }
        }
    }
}
