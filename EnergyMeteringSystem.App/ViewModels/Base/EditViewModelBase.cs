using EnergyMeteringSystem.App.Commands;
using EnergyMeteringSystem.App.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EnergyMeteringSystem.App.ViewModels.Base
{
    /// <summary>
    /// Базовый класс для ViewModel редактирования
    /// </summary>
    /// <typeparam name="TModel">Тип модели (DTO)</typeparam>
    /// <typeparam name="TRepository">Тип репозитория</typeparam>
    public abstract class EditViewModelBase<TModel, TRepository> : ValidatableViewModel
        where TModel : class, new()
        where TRepository : class
    {
        protected readonly TRepository _repository;
        protected TModel _originalItem;
        protected bool _isEditMode;
        private string _title;

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand SaveCommand { get; protected set; }
        public ICommand CancelCommand { get; protected set; }

        public event EventHandler OnSaved;
        public event EventHandler OnCancelled;

        protected EditViewModelBase(TRepository repository, TModel item = null)
        {
            _repository = repository;
            _originalItem = item;
            IsEditMode = item != null;
            Title = IsEditMode ? "Редактирование" : "Добавление";

            SaveCommand = new AsyncRelayCommand(async () => await SaveAsync(), () => CanSave());
            CancelCommand = new RelayCommand(_ => Cancel());

            if (IsEditMode)
                LoadItem(item);
        }

        protected abstract void LoadItem(TModel item);
        protected abstract TModel GetDto();
        protected abstract Task SaveToRepositoryAsync(TModel dto);
        protected abstract bool CanSave();

        protected virtual async Task SaveAsync()
        {
            if (!CanSave())
            {
                // Используем метод GetErrors из ValidatableViewModel
                var missingFieldsMessage = GetMissingFieldsMessage();
                if (!string.IsNullOrEmpty(missingFieldsMessage))
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageBox.Show(
                            $"Невозможно сохранить:\n\n{missingFieldsMessage}\n\nЗаполните все обязательные поля.",
                            "Ошибка валидации",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    });
                }
                return;
            }

            await ExecuteAsync(async () =>
            {
                var dto = GetDto();
                await SaveToRepositoryAsync(dto);
                RaiseOnSaved();
            }, "Ошибка при сохранении");
        }

        protected virtual void Cancel()
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }

        protected void RaiseOnSaved()
        {
            OnSaved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Получает сообщение о незаполненных обязательных полях
        /// </summary>
        protected virtual string GetMissingFieldsMessage()
        {
            if (_fieldValidations == null || _fieldValidations.Count == 0)
                return null;

            var missingFields = new System.Text.StringBuilder();
            foreach (var validation in _fieldValidations)
            {
                if (validation.Value != null && validation.Value.Count > 0)
                {
                    missingFields.AppendLine($"• {validation.Key}: {string.Join(", ", validation.Value)}");
                }
            }
            return missingFields.Length > 0 ? missingFields.ToString() : null;
        }

        /// <summary>
        /// Асинхронное выполнение с обработкой ошибок
        /// </summary>
        protected async Task ExecuteAsync(Func<Task> action, string errorTitle = "Ошибка")
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        ex.Message,
                        errorTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        }
    }
}