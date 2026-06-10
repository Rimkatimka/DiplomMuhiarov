using System;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Admin
{
    public class AuditLogViewModel : ViewModelBase
    {
        private readonly AuditRepository _auditRepository;
        private DateTime _fromDate;
        private DateTime _toDate;
        private string _searchText;

        private ObservableCollection<AuditLogDto> _logs;
        private ObservableCollection<AuditLogDto> _filteredLogs;

        public ObservableCollection<AuditLogDto> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }

        public ObservableCollection<AuditLogDto> FilteredLogs
        {
            get => _filteredLogs;
            set => SetProperty(ref _filteredLogs, value);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value))
                {
                    LoadData();
                }
            }
        }

        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value))
                {
                    LoadData();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public AuditLogViewModel()
        {
            _auditRepository = new AuditRepository();

            Logs = new ObservableCollection<AuditLogDto>();
            FilteredLogs = new ObservableCollection<AuditLogDto>();

            // ✅ ИСПРАВЛЕНО: используем текущие реальные даты
            _fromDate = DateTime.Today.AddDays(-30);  // 30 дней назад
            _toDate = DateTime.Today;                  // Сегодня

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: LoadData called, FromDate={FromDate:dd.MM.yyyy}, ToDate={ToDate:dd.MM.yyyy}");

                Logs.Clear();
                var list = _auditRepository.GetByDate(FromDate, ToDate);

                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: loaded {list.Count} logs");

                foreach (var log in list)
                {
                    Logs.Add(log);
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel LoadData error: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            try
            {
                FilteredLogs.Clear();

                var filtered = string.IsNullOrWhiteSpace(SearchText)
                    ? Logs
                    : new ObservableCollection<AuditLogDto>(
                        Logs.Where(l =>
                            (l.UserName?.Contains(SearchText) ?? false) ||
                            (l.ActionType?.Contains(SearchText) ?? false) ||
                            (l.TableName?.Contains(SearchText) ?? false) ||
                            (l.Details?.Contains(SearchText) ?? false)));

                foreach (var log in filtered)
                {
                    FilteredLogs.Add(log);
                }

                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel: filtered {FilteredLogs.Count} logs");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AuditLogViewModel ApplyFilter error: {ex.Message}");
            }
        }
    }
}