// EnergyMeteringSystem.App/ViewModels/Dynamic/DynamicEditViewModel.cs
using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.ViewModels.Base;
using EnergyMeteringSystem.Services.DynamicForms.Models;
using EnergyMeteringSystem.Services.DynamicForms.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EnergyMeteringSystem.App.ViewModels.Dynamic
{
    public class DynamicEditViewModel : ViewModelBase
    {
        private readonly IMetadataService _metadataService;
        private readonly IDynamicRepository _repository;
        private readonly IFormBuilder _formBuilder;

        private TableMetadata _metadata;
        private FormResult _formResult;
        private readonly int? _editId;
        private string _title;

        public event EventHandler OnSaved;
        public event EventHandler OnCancelled;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public Grid FormGrid => _formResult?.FormGrid;

        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public DynamicEditViewModel(
            string tableName,
            int? editId,
            IMetadataService metadataService,
            IDynamicRepository repository,
            IFormBuilder formBuilder)
        {
            _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _formBuilder = formBuilder ?? throw new ArgumentNullException(nameof(formBuilder));
            _editId = editId;

            Title = editId.HasValue
                ? $"Редактирование: {GetRussianTableName(tableName)}"
                : $"Добавление: {GetRussianTableName(tableName)}";

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => !IsLoading);
            CancelCommand = new RelayCommand(_ => OnCancelled?.Invoke(this, EventArgs.Empty));

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IsLoading))
                    (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            };

            _ = LoadDataAsync(tableName);
        }

        private async Task LoadDataAsync(string tableName)
        {
            await ExecuteAsync(async () =>
            {
                _metadata = await _metadataService.GetTableMetadataAsync(tableName);

                Dictionary<string, object> data = null;
                if (_editId.HasValue)
                {
                    data = await _repository.GetByIdAsync(tableName, _editId.Value);
                }

                _formResult = await _formBuilder.BuildFormAsync(_metadata, data);
                OnPropertyChanged(nameof(FormGrid));
                OnPropertyChanged(nameof(Title));
            }, "Ошибка загрузки данных");
        }

        private async Task SaveAsync()
        {
            await ExecuteAsync(async () =>
            {
                var values = _formBuilder.CollectDataFromForm(_formResult);

                foreach (var field in _formResult.RequiredFields)
                {
                    if (!values.ContainsKey(field) || values[field] == null
                        || (values[field] is string s && string.IsNullOrWhiteSpace(s)))
                    {
                        throw new InvalidOperationException($"Заполните обязательное поле: {GetFieldLabel(field)}");
                    }
                }

                if (_editId.HasValue)
                {
                    await _repository.UpdateAsync(_metadata.TableName, _editId.Value, values);
                }
                else
                {
                    await _repository.InsertAsync(_metadata.TableName, values);
                }

                OnSaved?.Invoke(this, EventArgs.Empty);
            }, "Ошибка при сохранении");

            if (HasError)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(ErrorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning));
            }
        }

        private string GetFieldLabel(string fieldName)
        {
            var column = _metadata?.Columns?.FirstOrDefault(c =>
                string.Equals(c.ColumnName, fieldName, StringComparison.OrdinalIgnoreCase));
            return column?.RussianName ?? fieldName;
        }

        private string GetRussianTableName(string tableName)
        {
            return tableName switch
            {
                "Region" => "Регионы",
                "City" => "Города",
                "Street" => "Улицы",
                "ObjectType" => "Типы объектов",
                "MeterType" => "Типы счетчиков",
                "MeterStatus" => "Статусы счетчиков",
                "ReadingStatus" => "Статусы показаний",
                "RejectionReason" => "Причины отклонения",
                "EnergySource" => "Источники энергии",
                "UserRole" => "Роли пользователей",
                "VerificationInterval" => "Интервалы поверки",
                _ => tableName
            };
        }
    }
}