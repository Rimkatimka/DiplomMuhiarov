using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.Core.Models.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.ViewModels.Base
{
    /// <summary>
    /// Базовый класс для ViewModel списков
    /// Поддерживает: загрузку, поиск, пагинацию, CRUD операции
    /// </summary>
    /// <typeparam name="TModel">Тип модели (DTO)</typeparam>
    /// <typeparam name="TRepository">Тип репозитория</typeparam>
    public abstract class ListViewModelBase<TModel, TRepository> : ViewModelBase
        where TModel : class, new()
        where TRepository : class
    {
        protected readonly TRepository _repository;
        protected CancellationTokenSource _cancellationTokenSource;

        private string _searchText;
        private TModel _selectedItem;
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _totalCount;
        private bool _hasNextPage;
        private bool _hasPreviousPage;

        // Основные коллекции (List - для производительности)
        private List<TModel> _itemsList = new();
        private List<TModel> _filteredItemsList = new();

        // Свойства для работы с List
        public List<TModel> ItemsList
        {
            get => _itemsList;
            set => SetProperty(ref _itemsList, value);
        }

        public List<TModel> FilteredItemsList
        {
            get => _filteredItemsList;
            set => SetProperty(ref _filteredItemsList, value);
        }

        // Свойства для обратной совместимости (ObservableCollection)
        // Используются в старых ViewModel, которые ожидают ObservableCollection
        public ObservableCollection<TModel> Items => new ObservableCollection<TModel>(ItemsList);
        public ObservableCollection<TModel> FilteredItems => new ObservableCollection<TModel>(FilteredItemsList);

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _currentPage = 1;
                    ApplyFilter();
                }
            }
        }

        public TModel SelectedItem
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

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                    _ = LoadDataAsync();
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    _currentPage = 1;
                    _ = LoadDataAsync();
                }
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public bool HasNextPage
        {
            get => _hasNextPage;
            set => SetProperty(ref _hasNextPage, value);
        }

        public bool HasPreviousPage
        {
            get => _hasPreviousPage;
            set => SetProperty(ref _hasPreviousPage, value);
        }

        public bool HasItems => ItemsList?.Count > 0;
        public bool HasFilteredItems => FilteredItemsList?.Count > 0;

        // Команды
        public ICommand RefreshCommand { get; protected set; }
        public ICommand AddCommand { get; protected set; }
        public ICommand EditCommand { get; protected set; }
        public ICommand DeleteCommand { get; protected set; }
        public ICommand NextPageCommand { get; protected set; }
        public ICommand PrevPageCommand { get; protected set; }
        public ICommand ClearSearchCommand { get; protected set; }

        protected ListViewModelBase(TRepository repository)
        {
            _repository = repository;
            _cancellationTokenSource = new CancellationTokenSource();

            ItemsList = new List<TModel>();
            FilteredItemsList = new List<TModel>();

            RefreshCommand = new AsyncRelayCommand(async () => await LoadDataAsync());
            AddCommand = new AsyncRelayCommand(async () => await AddAsync());
            EditCommand = new AsyncRelayCommand(async () => await EditAsync(), () => SelectedItem != null);
            DeleteCommand = new AsyncRelayCommand(async () => await DeleteAsync(), () => SelectedItem != null);
            NextPageCommand = new AsyncRelayCommand(async () => { if (HasNextPage) CurrentPage++; });
            PrevPageCommand = new AsyncRelayCommand(async () => { if (HasPreviousPage) CurrentPage--; });
            ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);

            System.Diagnostics.Debug.WriteLine("ConsumptionObjectListViewModel конструктор");
            // Асинхронная загрузка данных
            _ = LoadDataAsync();
        }

        protected abstract Task LoadDataAsync();
        protected abstract Task AddAsync();
        protected abstract Task EditAsync();
        protected abstract Task DeleteAsync();

        /// <summary>
        /// Применяет фильтр к ItemsList и обновляет FilteredItemsList
        /// </summary>
        protected virtual void ApplyFilter()
        {
            System.Diagnostics.Debug.WriteLine($"ApplyFilter: SearchText='{SearchText}', ItemsList.Count={ItemsList.Count}");

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? ItemsList.ToList()
                : ItemsList.Where(i => ItemMatchesSearch(i, SearchText)).ToList();

            System.Diagnostics.Debug.WriteLine($"ApplyFilter: отфильтровано {filtered.Count} объектов");

            FilteredItemsList = filtered;

            // Уведомляем об изменениях
            OnPropertyChanged(nameof(FilteredItemsList));
            OnPropertyChanged(nameof(FilteredItems));
            OnPropertyChanged(nameof(HasFilteredItems));
        }

        protected abstract bool ItemMatchesSearch(TModel item, string searchText);

        protected virtual void CancelPendingRequests()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}