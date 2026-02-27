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
                _ = SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                _ = SetProperty(ref _selectedMeter, value);
                EditCommand.RaiseCanExecuteChanged();
                ReplaceCommand.RaiseCanExecuteChanged();
            }
        }

        public MeterStatusDto SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _ = SetProperty(ref _selectedStatus, value);
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

            Meters = [];
            FilteredMeters = [];
            Statuses = [];

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddMeter());  // ← должна быть эта строка
            EditCommand = new RelayCommand(_ => EditMeter(), _ => SelectedMeter != null);
            ReplaceCommand = new RelayCommand(_ => ReplaceMeter(), _ => SelectedMeter != null);

            LoadData();
            LoadStatuses();
        }

        private void LoadData()
        {
            Meters.Clear();
            System.Collections.Generic.List<MeterDto> list = _meterRepository.GetAll();
            System.Diagnostics.Debug.WriteLine($"MeterListViewModel: загружено {list.Count} счетчиков");

            foreach (MeterDto meter in list)
            {
                Meters.Add(meter);
            }

            ApplyFilter();
        }

        private void LoadStatuses()
        {
            Statuses.Clear();

            // Добавляем "Все статусы" для фильтра
            Statuses.Add(new MeterStatusDto { Id = 0, Name = "Все статусы" });

            // Получаем список из репозитория (List<DirectoryDto>)
            System.Collections.Generic.List<DirectoryDto> list = _statusRepository.GetAll();

            // Преобразуем DirectoryDto в MeterStatusDto
            foreach (DirectoryDto item in list)
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
            try
            {
                System.Diagnostics.Debug.WriteLine("AddMeter: начало");

                MeterEditViewModel editViewModel = new();
                Views.Meters.MeterEditView editView = new(editViewModel);
                _ = editView.ShowDialog();

                LoadData(); // обновить список после закрытия

                System.Diagnostics.Debug.WriteLine("AddMeter: конец");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в AddMeter: {ex.Message}");
                _ = System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            FilteredMeters.Clear();

            System.Collections.Generic.IEnumerable<MeterDto> filtered = Meters.AsEnumerable();

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

            foreach (MeterDto meter in filtered)
            {
                FilteredMeters.Add(meter);
            }

            System.Diagnostics.Debug.WriteLine($"ApplyFilter: {FilteredMeters.Count} счетчиков после фильтра");
        }


        private void EditMeter()
        {
            if (SelectedMeter == null)
            {
                return;
            }

            // TODO: открыть форму редактирования счетчика
            _ = System.Windows.MessageBox.Show($"Редактирование счетчика {SelectedMeter.SerialNumber}", "Информация",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void ReplaceMeter()
        {
            if (SelectedMeter == null)
            {
                return;
            }

            // TODO: открыть форму замены счетчика
            _ = System.Windows.MessageBox.Show($"Замена счетчика {SelectedMeter.SerialNumber}", "Информация",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}