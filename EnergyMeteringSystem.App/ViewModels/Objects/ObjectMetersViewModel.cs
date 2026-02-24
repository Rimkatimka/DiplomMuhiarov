using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ObjectMetersViewModel : ViewModelBase
    {
        public event EventHandler CloseRequested;
        private readonly MeterRepository _meterRepository;
        private ConsumptionObjectDto _object;

        public ObservableCollection<MeterDto> Meters { get; set; }
        public RelayCommand CloseCommand { get; }

        public string ObjectTitle => _object?.Address ?? "Счётчики";

        public ObjectMetersViewModel(ConsumptionObjectDto selectedObject)
        {
            _object = selectedObject;
            _meterRepository = new MeterRepository();
            Meters = new ObservableCollection<MeterDto>();

            CloseCommand = new RelayCommand(_ => Close());

            LoadMeters();
        }

        private void LoadMeters()
        {
            if (_object == null) return;

            var list = _meterRepository.GetByObjectId(_object.Id);
            Meters.Clear();
            foreach (var meter in list)
                Meters.Add(meter);
        }

        private void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
