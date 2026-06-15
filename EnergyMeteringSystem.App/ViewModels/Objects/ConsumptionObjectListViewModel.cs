using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Core.Models.DTO;
using EnergyMeteringSystem.Data.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Objects
{
    public class ConsumptionObjectListViewModel : ViewModelBase
    {
        private readonly ConsumptionObjectRepository _repository;
        private readonly RegionRepository _regionRepository;
        private readonly CityRepository _cityRepository;
        private readonly StreetRepository _streetRepository;

        private string _searchText;
        private ConsumptionObjectDto _selectedItem;
        private bool _isLoading;

        // Коллекции для фильтров
        private ObservableCollection<RegionDto> _regions;
        private ObservableCollection<CityDto> _citiesFilter;
        private ObservableCollection<StreetDto> _streetsFilter;

        private RegionDto _selectedRegionFilter;
        private CityDto _selectedCityFilter;
        private StreetDto _selectedStreetFilter;

        public ObservableCollection<ConsumptionObjectDto> Items { get; set; }
        public ObservableCollection<ConsumptionObjectDto> FilteredItems { get; set; }

        // Коллекции для ComboBox фильтров
        public ObservableCollection<RegionDto> Regions
        {
            get => _regions;
            set => SetProperty(ref _regions, value);
        }

        public ObservableCollection<CityDto> CitiesFilter
        {
            get => _citiesFilter;
            set => SetProperty(ref _citiesFilter, value);
        }

        public ObservableCollection<StreetDto> StreetsFilter
        {
            get => _streetsFilter;
            set => SetProperty(ref _streetsFilter, value);
        }

        public RegionDto SelectedRegionFilter
        {
            get => _selectedRegionFilter;
            set
            {
                if (SetProperty(ref _selectedRegionFilter, value))
                {
                    // ✅ Синхронная загрузка городов (данные уже в БД, но нужно подгрузить)
                    LoadCitiesForFilter(value?.Id ?? 0);
                    SelectedCityFilter = null;
                    SelectedStreetFilter = null;
                    ApplyFilters();
                }
            }
        }

        public CityDto SelectedCityFilter
        {
            get => _selectedCityFilter;
            set
            {
                if (SetProperty(ref _selectedCityFilter, value))
                {
                    LoadStreetsForFilter(value?.Id ?? 0);
                    SelectedStreetFilter = null;
                    ApplyFilters();
                }
            }
        }

        public StreetDto SelectedStreetFilter
        {
            get => _selectedStreetFilter;
            set
            {
                if (SetProperty(ref _selectedStreetFilter, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public ConsumptionObjectDto SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ClearFiltersCommand { get; }
        public RelayCommand<ConsumptionObjectDto> ShowMetersCommand { get; }

        public ConsumptionObjectListViewModel()
        {
            _repository = new ConsumptionObjectRepository();
            _regionRepository = new RegionRepository();
            _cityRepository = new CityRepository();
            _streetRepository = new StreetRepository();

            Items = new ObservableCollection<ConsumptionObjectDto>();
            FilteredItems = new ObservableCollection<ConsumptionObjectDto>();
            Regions = new ObservableCollection<RegionDto>();
            CitiesFilter = new ObservableCollection<CityDto>();
            StreetsFilter = new ObservableCollection<StreetDto>();

            RefreshCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddCommand = new RelayCommand(_ => AddObject());
            EditCommand = new RelayCommand(_ => EditObject(), _ => SelectedItem != null);
            DeleteCommand = new RelayCommand(_ => DeleteObject(), _ => SelectedItem != null);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ShowMetersCommand = new RelayCommand<ConsumptionObjectDto>(obj => ShowMeters(obj));

            // Загружаем справочники для фильтров (асинхронно, но не блокируя UI)
            LoadFilterDirectories();

            Task.Run(async () => await LoadDataAsync());
        }

        // ✅ Загрузка справочников для фильтров (отдельно от основной таблицы)
        private async void LoadFilterDirectories()
        {
            var regions = await _regionRepository.GetAllAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Regions.Clear();
                foreach (var region in regions)
                    Regions.Add(region);
            });
        }

        // ✅ Загрузка городов для фильтра (синхронно после выбора региона)
        private async void LoadCitiesForFilter(int regionId)
        {
            if (regionId <= 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => CitiesFilter.Clear());
                return;
            }

            var cities = await _cityRepository.GetByRegionIdAsync(regionId);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                CitiesFilter.Clear();
                foreach (var city in cities)
                    CitiesFilter.Add(city);
            });
        }

        // ✅ Загрузка улиц для фильтра
        private async void LoadStreetsForFilter(int cityId)
        {
            if (cityId <= 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => StreetsFilter.Clear());
                return;
            }

            var streets = await _streetRepository.GetByCityIdAsync(cityId);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StreetsFilter.Clear();
                foreach (var street in streets)
                    StreetsFilter.Add(street);
            });
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                var list = await _repository.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Items.Clear();
                    foreach (var obj in list)
                        Items.Add(obj);
                    ApplyFilters();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ✅ Фильтрация — только синхронная (данные уже в памяти)
        private void ApplyFilters()
        {
            FilteredItems.Clear();

            var filtered = Items.AsEnumerable();

            // Поиск по адресу
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(o => o.Address.ToLower().Contains(SearchText.ToLower()));
            }

            // Фильтр по региону
            if (SelectedRegionFilter != null)
            {
                filtered = filtered.Where(o => o.RegionId == SelectedRegionFilter.Id);
            }

            // Фильтр по городу
            if (SelectedCityFilter != null)
            {
                filtered = filtered.Where(o => o.CityId == SelectedCityFilter.Id);
            }

            // Фильтр по улице
            if (SelectedStreetFilter != null)
            {
                filtered = filtered.Where(o => o.StreetId == SelectedStreetFilter.Id);
            }

            foreach (var obj in filtered)
                FilteredItems.Add(obj);
        }

        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedRegionFilter = null;
            SelectedCityFilter = null;
            SelectedStreetFilter = null;

            // Очищаем коллекции фильтров
            CitiesFilter.Clear();
            StreetsFilter.Clear();

            ApplyFilters();
        }

        private void AddObject()
        {
            var editViewModel = new ConsumptionObjectEditViewModel();
            var editView = new Views.Objects.ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnObjectSaved += async (s, e) =>
            {
                await LoadDataAsync();
                editView.Close();
            };
            editView.ShowDialog();
        }

        private void EditObject()
        {
            if (SelectedItem == null) return;

            var editViewModel = new ConsumptionObjectEditViewModel(SelectedItem);
            var editView = new Views.Objects.ConsumptionObjectEditView(editViewModel);
            editView.Owner = Application.Current.MainWindow;

            editViewModel.OnObjectSaved += async (s, e) =>
            {
                await LoadDataAsync();
                editView.Close();
            };
            editView.ShowDialog();
        }

        private async void DeleteObject()
        {
            if (SelectedItem == null) return;

            var result = MessageBox.Show(
                $"Удалить объект \"{SelectedItem.Address}\"?\n\nВсе связанные данные также будут удалены!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedItem.Id);
                    await LoadDataAsync();
                    MessageBox.Show("Объект удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void ShowMeters(ConsumptionObjectDto obj)
        {
            if (obj == null) return;

            var window = new Views.Meters.MetersForObjectView();
            var viewModel = new ViewModels.Meters.MetersForObjectViewModel(obj);
            window.DataContext = viewModel;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }
}