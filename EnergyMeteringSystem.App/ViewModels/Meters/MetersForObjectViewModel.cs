using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Meters
{
    public class MetersForObjectViewModel : ViewModelBase
    {
        private readonly MeterRepository _meterRepository;
        private ConsumptionObjectDto _currentObject;

        private ObservableCollection<MeterDto> _meters;
        private MeterDto _selectedMeter;

        public ObservableCollection<MeterDto> Meters
        {
            get => _meters;
            set => SetProperty(ref _meters, value);
        }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                SetProperty(ref _selectedMeter, value);
                EditMeterCommand.RaiseCanExecuteChanged();
                DeleteMeterCommand.RaiseCanExecuteChanged();
            }
        }

        public string ObjectAddress => _currentObject?.Address ?? "Объект не выбран";

        public RelayCommand AddMeterCommand { get; }
        public RelayCommand EditMeterCommand { get; }
        public RelayCommand DeleteMeterCommand { get; }

        public MetersForObjectViewModel(ConsumptionObjectDto selectedObject)
        {
            _currentObject = selectedObject;
            _meterRepository = new MeterRepository();
            Meters = new ObservableCollection<MeterDto>();

            AddMeterCommand = new RelayCommand(_ => AddMeter());
            EditMeterCommand = new RelayCommand(_ => EditMeter(), _ => SelectedMeter != null);
            DeleteMeterCommand = new RelayCommand(_ => DeleteMeter(), _ => SelectedMeter != null);

            LoadMeters();
        }

        private void LoadMeters()
        {
            System.Diagnostics.Debug.WriteLine($"LoadMeters: objectId={_currentObject?.Id}");

            var meters = _meterRepository.GetByObjectId(_currentObject.Id);
            System.Diagnostics.Debug.WriteLine($"LoadMeters: получили {meters.Count} счетчиков");

            Meters.Clear();
            foreach (var m in meters)
            {
                // Полная отладка свойств
                System.Diagnostics.Debug.WriteLine($"  Meter: Id={m.Id}");
                System.Diagnostics.Debug.WriteLine($"    SerialNumber: {m.SerialNumber}");
                System.Diagnostics.Debug.WriteLine($"    MeterTypeName: {m.MeterTypeName}");
                System.Diagnostics.Debug.WriteLine($"    StatusName: {m.StatusName}");
                System.Diagnostics.Debug.WriteLine($"    InstallationDate: {m.InstallationDate}");
                System.Diagnostics.Debug.WriteLine($"    NextVerificationDate: {m.NextVerificationDate}");
                System.Diagnostics.Debug.WriteLine($"    ServiceLifeYears: {m.ServiceLifeYears}");
                System.Diagnostics.Debug.WriteLine($"    RemovalDate: {m.RemovalDate}");

                Meters.Add(m);
            }

            OnPropertyChanged(nameof(Meters));
        }

        private void AddMeter()
        {
            System.Diagnostics.Debug.WriteLine($"AddMeter: _currentObject = {_currentObject?.Id}");

            if (_currentObject == null)
            {
                System.Diagnostics.Debug.WriteLine("ОШИБКА: _currentObject = null, нельзя добавить счётчик");
                return;
            }

            var editViewModel = new MeterEditViewModel(_currentObject);
            var editView = new Views.Meters.MeterEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            editViewModel.OnMeterSaved += (s, e) =>
            {
                LoadMeters();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void EditMeter()
        {
            if (SelectedMeter == null) return;

            var editViewModel = new MeterEditViewModel(SelectedMeter);
            var editView = new Views.Meters.MeterEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            editViewModel.OnMeterSaved += (s, e) =>
            {
                LoadMeters();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private void DeleteMeter()
        {
            if (SelectedMeter == null) return;

            var result = MessageBox.Show($"Удалить счётчик {SelectedMeter.SerialNumber}?",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _meterRepository.Delete(SelectedMeter.Id);
                LoadMeters();
            }
        }
    }
}