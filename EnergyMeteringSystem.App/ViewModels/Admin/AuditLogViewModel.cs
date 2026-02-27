using System;
using System.Collections.Generic;
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

        public ObservableCollection<AuditLogDto> Logs { get; set; }
        public ObservableCollection<AuditLogDto> FilteredLogs { get; set; }

        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                _ = SetProperty(ref _fromDate, value);
                LoadData(); // ← должно вызываться!
            }
        }

        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                _ = SetProperty(ref _toDate, value);
                LoadData(); // ← должно вызываться!
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _ = SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand ClearFilterCommand { get; }

        public AuditLogViewModel()
        {
            _auditRepository = new AuditRepository();

            Logs = [];
            FilteredLogs = [];

            _fromDate = DateTime.Today.AddDays(-7);
            _toDate = DateTime.Today;

            RefreshCommand = new RelayCommand(_ => LoadData());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());

            LoadData(); // ← вызываем при создании
        }


        private void LoadData()
        {
            Logs.Clear();
            List<AuditLogDto> list = _auditRepository.GetByDate(_fromDate, _toDate);
            System.Diagnostics.Debug.WriteLine($"AuditLog: loaded {list.Count} logs");

            foreach (AuditLogDto log in list)
            {
                Logs.Add(log);
            }

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredLogs.Clear();

            ObservableCollection<AuditLogDto> filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Logs
                : [.. Logs.Where(l =>
                        l.UserName.Contains(SearchText) ||
                        l.ActionType.Contains(SearchText) ||
                        l.TableName.Contains(SearchText) ||
                        l.Details.Contains(SearchText))];

            foreach (AuditLogDto log in filtered)
            {
                FilteredLogs.Add(log);
            }

            System.Diagnostics.Debug.WriteLine($"ApplyFilter: {FilteredLogs.Count} logs");
        }

        private void ClearFilter()
        {
            SearchText = string.Empty;
            FromDate = DateTime.Today.AddDays(-7);
            ToDate = DateTime.Today;
        }
    }
}