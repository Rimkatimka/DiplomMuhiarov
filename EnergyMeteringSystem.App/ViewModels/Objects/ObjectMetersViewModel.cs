using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ObjectMetersViewModel : ViewModelBase
    {
        private readonly MeterRepository _meterRepository;
        private readonly ConsumptionObjectDto _object;

        public ObservableCollection<MeterDto> Meters { get; set; }
        public RelayCommand CloseCommand { get; }

        public string ObjectTitle => _object?.Address ?? "Счетчики объекта";

        public ObjectMetersViewModel(ConsumptionObjectDto selectedObject)
        {
            _object = selectedObject;
            _meterRepository = new MeterRepository();
            Meters = new ObservableCollection<MeterDto>();

            CloseCommand = new RelayCommand(_ => Close());

            _ = LoadMetersAsync();
        }

        private async Task LoadMetersAsync()
        {
            await ExecuteAsync(async () =>
            {
                if (_object == null) return;

                var list = await _meterRepository.GetByObjectIdAsync(_object.Id);
                Meters.Clear();
                foreach (var meter in list)
                    Meters.Add(meter);
            }, "Ошибка загрузки счетчиков");
        }

        private void Close()
        {
            var window = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this);
            window?.Close();
        }
    }
}