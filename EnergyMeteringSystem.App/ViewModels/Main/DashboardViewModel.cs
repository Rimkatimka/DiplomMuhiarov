using System;
using System.Collections.ObjectModel;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Main
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly DashboardRepository _dashboardRepository;
        private DashboardDto _data;

        public DashboardViewModel()
        {
            _dashboardRepository = new DashboardRepository();
            TopDebtors = new ObservableCollection<DebtDto>();
            ChartData = new ObservableCollection<ChartPoint>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            LoadData();
        }

        // Основные показатели
        public int TotalObjects => _data?.TotalObjects ?? 0;
        public int TotalMeters => _data?.TotalMeters ?? 0;
        public int ReadingsToday => _data?.ReadingsToday ?? 0;
        public int ReadingsWeek => _data?.ReadingsWeek ?? 0;
        public string AccrualMonth => (_data?.AccrualMonth ?? 0).ToString("F0") + " ₽";
        public string PaymentMonth => (_data?.PaymentMonth ?? 0).ToString("F0") + " ₽";
        public int ExpiredMeters => _data?.ExpiredMeters ?? 0;

        // Коллекции
        public ObservableCollection<DebtDto> TopDebtors { get; set; }
        public ObservableCollection<ChartPoint> ChartData { get; set; }

        // Команда обновления
        public RelayCommand RefreshCommand { get; }

        private void LoadData()
        {
            _data = _dashboardRepository.GetDashboardData();

            // Обновляем свойства
            OnPropertyChanged(nameof(TotalObjects));
            OnPropertyChanged(nameof(TotalMeters));
            OnPropertyChanged(nameof(ReadingsToday));
            OnPropertyChanged(nameof(ReadingsWeek));
            OnPropertyChanged(nameof(AccrualMonth));
            OnPropertyChanged(nameof(PaymentMonth));
            OnPropertyChanged(nameof(ExpiredMeters));

            // Обновляем коллекции
            TopDebtors.Clear();
            if (_data?.TopDebtors != null)
            {
                foreach (var debtor in _data.TopDebtors)
                    TopDebtors.Add(debtor);
            }

            ChartData.Clear();
            if (_data?.ConsumptionChart != null)
            {
                foreach (var point in _data.ConsumptionChart)
                    ChartData.Add(point);
            }
        }
    }
}