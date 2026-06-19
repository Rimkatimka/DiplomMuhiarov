using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.App.ViewModels.Dynamic;
using EnergyMeteringSystem.Services.DynamicForms.Models;
using EnergyMeteringSystem.Services.DynamicForms.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EnergyMeteringSystem.App.ViewModels.Directories
{
    /// <summary>
    /// Универсальная ViewModel для работы с динамическими справочниками.
    /// </summary>
    public class DynamicDirectoryListViewModel : ViewModelBase
    {
        private readonly IMetadataService _metadataService;
        private readonly IDynamicRepository _repository;
        private readonly IFormBuilder _formBuilder;

        private DataTable _dataTable;
        private DataView _gridData;
        private DataRowView _selectedRow;
        private string _selectedTableName;
        private string _searchText;

        public ObservableCollection<TableInfo> AvailableTables { get; } = new();
        public ObservableCollection<ColumnDisplayInfo> Columns { get; } = new();

        public DataView GridData
        {
            get => _gridData;
            private set
            {
                if (SetProperty(ref _gridData, value))
                    OnPropertyChanged(nameof(HasItems));
            }
        }

        public DataRowView SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (SetProperty(ref _selectedRow, value))
                    RefreshSelectionCommands();
            }
        }

        public string SelectedTableName
        {
            get => _selectedTableName;
            set
            {
                if (SetProperty(ref _selectedTableName, value) && !string.IsNullOrEmpty(value))
                    _ = LoadTableDataAsync(value);
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ApplyFilter();
            }
        }

        public bool HasItems => GridData != null && GridData.Count > 0;

        public string CurrentTableRussianName
        {
            get => _currentTableRussianName;
            private set => SetProperty(ref _currentTableRussianName, value);
        }
        private string _currentTableRussianName;

        public AsyncRelayCommand RefreshCommand { get; }
        public AsyncRelayCommand AddCommand { get; }
        public AsyncRelayCommand EditCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }

        public DynamicDirectoryListViewModel(
            IMetadataService metadataService,
            IDynamicRepository repository,
            IFormBuilder formBuilder)
        {
            _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _formBuilder = formBuilder ?? throw new ArgumentNullException(nameof(formBuilder));

            RefreshCommand = new AsyncRelayCommand(async () => await LoadTablesAsync(), () => !IsLoading);
            AddCommand = new AsyncRelayCommand(async () => await AddAsync(), () => !IsLoading && !string.IsNullOrEmpty(SelectedTableName));
            EditCommand = new AsyncRelayCommand(async () => await EditAsync(), CanExecuteSelectionCommand);
            DeleteCommand = new AsyncRelayCommand(async () => await DeleteAsync(), CanExecuteSelectionCommand);

            PropertyChanged += OnViewModelPropertyChanged;

            _ = LoadTablesAsync();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsLoading))
                RefreshAllCommandsCanExecute();
        }

        private bool CanExecuteSelectionCommand() => !IsLoading && SelectedRow != null;

        private void RefreshSelectionCommands()
        {
            (EditCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (DeleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private void RefreshAllCommandsCanExecute()
        {
            (RefreshCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (AddCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            RefreshSelectionCommands();
            (DeleteCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task LoadTablesAsync()
        {
            await ExecuteAsync(async () =>
            {
                var tableNames = await _metadataService.GetAllTableNamesAsync();

                AvailableTables.Clear();
                foreach (var name in tableNames)
                {
                    var metadata = await _metadataService.GetTableMetadataAsync(name);
                    AvailableTables.Add(new TableInfo
                    {
                        TableName = name,
                        RussianName = metadata.RussianName
                    });
                }

                if (AvailableTables.Any())
                    SelectedTableName = AvailableTables.First().TableName;
            }, "Ошибка загрузки списка справочников");

            if (HasError)
                await ShowErrorAsync(ErrorMessage);
        }

        private async Task LoadTableDataAsync(string tableName)
        {
            await ExecuteAsync(async () =>
            {
                var metadata = await _metadataService.GetTableMetadataAsync(tableName);
                CurrentTableRussianName = metadata.RussianName;

                Columns.Clear();
                var addedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var column in metadata.Columns.Where(c => !c.IsForeignKey))
                {
                    if (!addedColumns.Add(column.ColumnName))
                        continue;

                    Columns.Add(new ColumnDisplayInfo
                    {
                        ColumnName = column.ColumnName,
                        RussianName = column.RussianName,
                        DataType = column.DataType,
                        IsVisible = true
                    });
                }

                _dataTable = await _repository.GetAllAsDataTableAsync(tableName);
                ApplyFilter();
            }, $"Ошибка загрузки {CurrentTableRussianName ?? tableName}");

            // Если произошла ошибка – показываем её в UI
            if (HasError)
            {
                await ShowErrorAsync(ErrorMessage);
            }
        }

        private void ApplyFilter()
        {
            if (_dataTable == null)
            {
                GridData = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                GridData = _dataTable.DefaultView;
                return;
            }

            var filteredTable = _dataTable.Clone();
            var searchLower = SearchText.ToLower();

            foreach (DataRow row in _dataTable.Rows)
            {
                bool matches = false;
                foreach (DataColumn column in _dataTable.Columns)
                {
                    if (row[column] == DBNull.Value)
                        continue;

                    if (row[column].ToString().ToLower().Contains(searchLower))
                    {
                        matches = true;
                        break;
                    }
                }

                if (matches)
                    filteredTable.ImportRow(row);
            }

            GridData = filteredTable.DefaultView;
        }

        private int? GetSelectedId()
        {
            if (SelectedRow == null)
                return null;

            var idValue = SelectedRow.Row.Table.Columns.Contains("Id")
                ? SelectedRow["Id"]
                : null;

            if (idValue == null || idValue == DBNull.Value)
                return null;

            return Convert.ToInt32(idValue);
        }

        private string GetSelectedDisplayName(int id)
        {
            if (SelectedRow == null)
                return $"ID: {id}";

            if (SelectedRow.Row.Table.Columns.Contains("Name"))
            {
                var name = SelectedRow["Name"];
                if (name != null && name != DBNull.Value)
                    return name.ToString();
            }

            return $"ID: {id}";
        }

        private async Task AddAsync()
        {
            if (string.IsNullOrEmpty(SelectedTableName))
                return;

            try
            {
                var viewModel = new DynamicEditViewModel(
                    SelectedTableName,
                    null,
                    _metadataService,
                    _repository,
                    _formBuilder);

                var view = new Views.Dynamic.DynamicEditView(viewModel);
                view.Owner = Application.Current.MainWindow;

                if (view.ShowDialog() == true)
                    await LoadTableDataAsync(SelectedTableName);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка добавления: {ex.Message}");
            }
        }

        private async Task EditAsync()
        {
            var id = GetSelectedId();
            if (!id.HasValue)
                return;

            try
            {
                var viewModel = new DynamicEditViewModel(
                    SelectedTableName,
                    id.Value,
                    _metadataService,
                    _repository,
                    _formBuilder);

                var view = new Views.Dynamic.DynamicEditView(viewModel);
                view.Owner = Application.Current.MainWindow;

                if (view.ShowDialog() == true)
                    await LoadTableDataAsync(SelectedTableName);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Ошибка редактирования: {ex.Message}");
            }
        }

        private async Task DeleteAsync()
        {
            var id = GetSelectedId();
            if (!id.HasValue)
                return;

            var name = GetSelectedDisplayName(id.Value);
            var result = MessageBox.Show(
                $"Удалить запись \"{name}\"?",
                $"Подтверждение удаления - {CurrentTableRussianName}",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            await ExecuteAsync(async () =>
            {
                await _repository.DeleteAsync(SelectedTableName, id.Value);
                await LoadTableDataAsync(SelectedTableName);
            }, "Ошибка при удалении");

            if (HasError)
                await ShowErrorAsync(ErrorMessage);
        }

        private Task ShowErrorAsync(string message)
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }).Task;
        }
    }

    public class TableInfo
    {
        public string TableName { get; set; }
        public string RussianName { get; set; }
    }

    public class ColumnDisplayInfo
    {
        public string ColumnName { get; set; }
        public string RussianName { get; set; }
        public string DataType { get; set; }
        public bool IsVisible { get; set; }
    }
}
