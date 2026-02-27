using System;
using System.Collections.ObjectModel;
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
            System.Diagnostics.Debug.WriteLine("ObjectMetersViewModel конструктор начат");

            _object = selectedObject;
            _meterRepository = new MeterRepository();
            Meters = [];

            CloseCommand = new RelayCommand(_ =>
            {
                // Если это окно, можно закрыть
                if (Application.Current.Windows.Count > 0)
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window.DataContext == this)
                        {
                            window.Close();
                            return;
                        }
                    }
                }
            });

            LoadMeters();

            System.Diagnostics.Debug.WriteLine("ObjectMetersViewModel конструктор завершен");
        }

        private void LoadMeters()
        {
            try
            {
                if (_object == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadMeters: _object = null");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"LoadMeters: загружаем счетчики для объекта ID={_object.Id}, адрес={_object.Address}");

                Meters.Clear();
                System.Collections.Generic.List<MeterDto> list = _meterRepository.GetByObjectId(_object.Id);

                System.Diagnostics.Debug.WriteLine($"LoadMeters: получено {list.Count} счетчиков из репозитория");

                foreach (MeterDto meter in list)
                {
                    Meters.Add(meter);
                    System.Diagnostics.Debug.WriteLine($"  - Добавлен счетчик: {meter.SerialNumber}, тип: {meter.MeterTypeName}");
                }

                if (Meters.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("LoadMeters: нет счетчиков для отображения");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в LoadMeters: {ex.Message}");
                _ = MessageBox.Show($"Ошибка загрузки счетчиков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}