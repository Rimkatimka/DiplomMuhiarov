using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private MeterDto _selectedMeter;

        public ObservableCollection<MeterDto> Meters { get; set; }

        public MeterDto SelectedMeter
        {
            get => _selectedMeter;
            set
            {
                SetProperty(ref _selectedMeter, value);
                (EditMeterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (DeleteMeterCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ObjectAddress => _currentObject?.Address ?? "Объект не выбран";

        public AsyncRelayCommand AddMeterCommand { get; }
        public AsyncRelayCommand EditMeterCommand { get; }
        public AsyncRelayCommand DeleteMeterCommand { get; }
        public RelayCommand CloseCommand { get; }

        public MetersForObjectViewModel(ConsumptionObjectDto selectedObject)
        {
            _currentObject = selectedObject;
            _meterRepository = new MeterRepository();
            Meters = new ObservableCollection<MeterDto>();

            AddMeterCommand = new AsyncRelayCommand(async () => await AddMeterAsync());
            EditMeterCommand = new AsyncRelayCommand(async () => await EditMeterAsync(), () => SelectedMeter != null);
            DeleteMeterCommand = new AsyncRelayCommand(async () => await DeleteMeterAsync(), () => SelectedMeter != null);
            CloseCommand = new RelayCommand(_ => Close());

            _ = LoadMetersAsync();
        }

        private async Task LoadMetersAsync()
        {
            await ExecuteAsync(async () =>
            {
                System.Diagnostics.Debug.WriteLine($"LoadMetersAsync: objectId={_currentObject?.Id}");

                var meters = await _meterRepository.GetByObjectIdAsync(_currentObject.Id);
                System.Diagnostics.Debug.WriteLine($"LoadMetersAsync: получили {meters.Count} счетчиков");

                Meters.Clear();
                foreach (var m in meters)
                {
                    Meters.Add(m);
                }
            }, "Ошибка загрузки счетчиков");
        }

        private async Task AddMeterAsync()
        {
            if (_currentObject == null)
            {
                System.Diagnostics.Debug.WriteLine("ОШИБКА: _currentObject = null, нельзя добавить счётчик");
                return;
            }

            var editViewModel = new MeterEditViewModel(_currentObject);
            var editView = new Views.Meters.MeterEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            editViewModel.OnSaved += async (s, e) =>
            {
                await LoadMetersAsync();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private async Task EditMeterAsync()
        {
            if (SelectedMeter == null) return;

            var editViewModel = new MeterEditViewModel(SelectedMeter);
            var editView = new Views.Meters.MeterEditView(editViewModel);
            editView.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);

            editViewModel.OnSaved += async (s, e) =>
            {
                await LoadMetersAsync();
                editView.Close();
            };

            editView.ShowDialog();
        }

        private async Task DeleteMeterAsync()
        {
            if (SelectedMeter == null) return;

            var result = MessageBox.Show($"Удалить счётчик {SelectedMeter.SerialNumber}?",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await ExecuteAsync(async () =>
                {
                    await _meterRepository.DeleteAsync(SelectedMeter.Id);
                    await LoadMetersAsync();
                }, "Ошибка при удалении");
            }
        }

        private void Close()
        {
            var window = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}