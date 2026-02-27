using System;
using System.Collections.ObjectModel;
using System.Linq;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Meters
{
    public class MeterListViewModel : ViewModelBase
    {
        private readonly MeterRepository _meterRepository;
        private readonly MeterStatusRepository _statusRepository;

        private string _searchText;
        private MeterDto _selectedMeter;
        private MeterStatusDto _selectedStatus;

        public ObservableCollection<MeterDto> Meters { get; set; }
        public ObservableCollection<MeterDto> FilteredMeters { get; set; }
        public ObservableCollection<MeterStatusDto> Statuses { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                SetProperty(ref _selectedMeter, value);
                EditCommand.RaiseCanExecuteChanged();
                ReplaceCommand.RaiseCanExecuteChanged();
            }
        }

        public MeterStatusDto SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                SetProperty(ref _selectedStatus, value);
                ApplyFilter();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand ReplaceCommand { get; }

        public MeterListViewModel()
        {
            _meterRepository = new MeterRepository();
            _statusRepository = new MeterStatusRepository();

            Meters = new ObservableCollection<MeterDto>();
            FilteredMeters = new ObservableCollection<MeterDto>();
            Statuses = new ObservableCollection<MeterStatusDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddMeter());
            EditCommand = new RelayCommand(_ => EditMeter(), _ => SelectedMeter != null);
            ReplaceCommand = new RelayCommand(_ => ReplaceMeter(), _ => SelectedMeter != null);

            LoadData();
            LoadStatuses();
        }

        private void LoadData()
        {
            Meters.Clear();
            var list = _meterRepository.GetAll();
            System.Diagnostics.Debug.WriteLine($"MeterListViewModel: загружено {list.Count} счетчиков");

            foreach (var meter in list)
                Meters.Add(meter);

            ApplyFilter();
        }

        private void LoadStatuses()
        {
            Statuses.Clear();

            // Добавляем "Все статусы" для фильтра
            Statuses.Add(new MeterStatusDto { Id = 0, Name = "Все статусы" });

            // Получаем список из репозитория (List<DirectoryDto>)
            var list = _statusRepository.GetAll();

            // Преобразуем DirectoryDto в MeterStatusDto
            foreach (var item in list)
            {
                Statuses.Add(new MeterStatusDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description
                });
            }
        }
        private void AddMeter()
        {
            var editViewModel = new MeterEditViewModel();
            var editView = new Views.Meters.MeterEditView(editViewModel);
            editView.ShowDialog();

            LoadData(); // обновить список после закрытия
        }

        private void ApplyFilter()
        {
            FilteredMeters.Clear();

            var filtered = Meters.AsEnumerable();

            // Фильтр по тексту (серийный номер)
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(m =>
                    m.SerialNumber.Contains(SearchText));
            }

            // Фильтр по статусу
            if (SelectedStatus != null && SelectedStatus.Id > 0)
            {
                filtered = filtered.Where(m =>
                    m.StatusId == SelectedStatus.Id);
            }

            foreach (var meter in filtered)
                FilteredMeters.Add(meter);

            System.Diagnostics.Debug.WriteLine($"ApplyFilter: {FilteredMeters.Count} счетчиков после фильтра");
        }


        private void EditMeter()
        {
            if (SelectedMeter == null) return;

            // TODO: открыть форму редактирования счетчика
            System.Windows.MessageBox.Show($"Редактирование счетчика {SelectedMeter.SerialNumber}", "Информация",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void ReplaceMeter()
        {
            if (SelectedMeter == null) return;

            // TODO: открыть форму замены счетчика
            System.Windows.MessageBox.Show($"Замена счетчика {SelectedMeter.SerialNumber}", "Информация",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}