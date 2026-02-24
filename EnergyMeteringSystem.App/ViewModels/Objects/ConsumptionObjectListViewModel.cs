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
    public class ConsumptionObjectListViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _repository;
        private string _searchText;
        public RelayCommand<ConsumptionObjectDto> ShowMetersCommand { get; private set; }

        public ObservableCollection<ConsumptionObjectDto> Objects { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredObjects { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand<ConsumptionObjectDto> EditCommand { get; }

        public ConsumptionObjectListViewModel()
        {
            _repository = new ConsumptionObjectRepository();
            Objects = new ObservableCollection<ConsumptionObjectDto>();
            FilteredObjects = new ObservableCollection<ConsumptionObjectDto>();

            RefreshCommand = new RelayCommand(_ => LoadData());
            AddCommand = new RelayCommand(_ => AddObject());
            EditCommand = new RelayCommand<ConsumptionObjectDto>(EditObject);

            ShowMetersCommand = new RelayCommand<ConsumptionObjectDto>(ShowMeters);

            LoadData();
        }

        private void LoadData()
        {
            var list = _repository.GetAll();
            Objects.Clear();
            foreach (var obj in list)
                Objects.Add(obj);

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredObjects.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Objects
                : new ObservableCollection<ConsumptionObjectDto>(
                    Objects.Where(o =>
                        o.Name.Contains(SearchText) ||
                        o.Address.Contains(SearchText))
                );

            foreach (var obj in filtered)
                FilteredObjects.Add(obj);
        }

        private void AddObject()
        {
            // позже
        }

        private void EditObject(ConsumptionObjectDto obj)
        {
            // позже
        }
        private void ShowMeters(ConsumptionObjectDto obj)
        {
            if (obj == null) return;

            var window = new Views.Objects.ObjectMetersView(obj);
            window.ShowDialog();

        }

    }
}
