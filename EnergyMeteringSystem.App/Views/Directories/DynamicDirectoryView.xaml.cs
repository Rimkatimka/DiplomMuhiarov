using EnergyMeteringSystem.App.ViewModels.Directories;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.Views.Directories
{
    public partial class DynamicDirectoryView : UserControl
    {
        private DynamicDirectoryListViewModel _viewModel;

        public DynamicDirectoryView(DynamicDirectoryListViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Columns.CollectionChanged -= Columns_CollectionChanged;
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _viewModel = DataContext as DynamicDirectoryListViewModel;
            if (_viewModel == null)
                return;

            _viewModel.Columns.CollectionChanged += Columns_CollectionChanged;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            RefreshGrid();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DynamicDirectoryListViewModel.GridData)
                || e.PropertyName == nameof(DynamicDirectoryListViewModel.SelectedTableName))
            {
                RefreshGrid();
            }
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            DirectoryDataGrid.Columns.Clear();
            DirectoryDataGrid.ItemsSource = null;
            DirectoryDataGrid.ItemsSource = _viewModel?.GridData;
        }

        private void DirectoryDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Если метаданные ещё не загружены – не блокируем создание колонок
            if (_viewModel?.Columns == null || _viewModel.Columns.Count == 0)
            {
                // Оставляем колонку с автоматическим заголовком
                return;
            }

            var columnInfo = _viewModel.Columns.FirstOrDefault(c =>
                string.Equals(c.ColumnName, e.PropertyName, System.StringComparison.OrdinalIgnoreCase));

            if (columnInfo == null)
            {
                // Если информация о колонке не найдена – создаём колонку с именем свойства
                e.Column.Header = e.PropertyName;
                return;
            }

            if (!columnInfo.IsVisible)
            {
                e.Cancel = true;
                return;
            }

            // Устанавливаем заголовок (русское имя или оригинальное)
            e.Column.Header = string.IsNullOrWhiteSpace(columnInfo.RussianName)
                ? columnInfo.ColumnName
                : columnInfo.RussianName;

            // Дополнительная настройка ширины для Id
            if (string.Equals(columnInfo.ColumnName, "Id", StringComparison.OrdinalIgnoreCase))
                e.Column.Width = 60;
        }

    }
}
